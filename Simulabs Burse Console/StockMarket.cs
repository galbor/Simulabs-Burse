using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Simulabs_Burse_Console.Factories;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console
{
    public class StockMarket : IStockMarket
    {
        private IStockMarketFactory factory = new RegularStockMarketFactory();

        private readonly Dictionary<string, ICompany> _companies = new Dictionary<string, ICompany>();
        private readonly Dictionary<string, ITrader> _traders = new Dictionary<string, ITrader>();

        private readonly Dictionary<string, List<IOffer>> _allTraderOffers = new Dictionary<string, List<IOffer>>(); //traderId -> list(offer)
        private readonly Dictionary<string, SortedSet<IOffer>> _allSellCompanyOffers = new Dictionary<string, SortedSet<IOffer>>(); //companyId -> set(offer)
        private readonly Dictionary<string, SortedSet<IOffer>> _allBuyCompanyOffers = new Dictionary<string, SortedSet<IOffer>>(); //companyId -> set(offer)

        private readonly List<IOffer> _pendingOffers = new List<IOffer>();
        private readonly List<IOffer> _pendingDeleteOffers = new List<IOffer>();

        private bool _run = false;

        private static object _lock = new Object();
        private bool _isDoingWork = false;

        private static IStockMarket _instance = null;

        public static IStockMarket Instance
        {
            get { return GetInstance(); }
        }

        private StockMarket()
        {
        }

        private static IStockMarket GetInstance()
        {
            if (_instance != null) return _instance;
            lock (_lock)
            {
                if (_instance != null) return _instance;
                _instance = new StockMarket();
            }
            return _instance;
        }

        public List<ICompany> GetAllCompanies()
        {
            return _companies.Values.ToList();
        }

        public List<ITrader> GetAllTraders()
        {
            return _traders.Values.ToList();
        }

        /**
         * gets the offers list of an id
         * if doesn't exist in dictionary, creates an empty list
         */
        private static List<IOffer> GetOfferList(Dictionary<string, List<IOffer>> offers, string id)
        {
            if (!offers.TryGetValue(id, out List<IOffer> res))
            {
                res = new List<IOffer>();
                offers.Add(id, res);
            }

            return res;
        }

        private static SortedSet<IOffer> GetOfferSet(Dictionary<string, SortedSet<IOffer>> offers, string id)
        {
            if (!offers.TryGetValue(id, out SortedSet<IOffer> res))
            {
                res = new SortedSet<IOffer>();
                offers.Add(id, res);
            }

            return res;
        }

        private static IOffer[] GetOffersAsArray(Dictionary<string, List<IOffer>> offersDict, string id)
        {
            var offers = GetOfferList(offersDict, id);
            IOffer[] res = new IOffer[offers.Count];
            offers.CopyTo(res);
            res = res.Where(IsLegalOffer).ToArray();
            return res;
        }

        private static IOffer[] GetOffersAsArray(Dictionary<string, SortedSet<IOffer>> offersDict, string id)
        {
            var offers = GetOfferSet(offersDict, id);

            IOffer[] res = new IOffer[offers.Count];
            offers.CopyTo(res);
            res = res.Where(IsLegalOffer).ToArray();
            return res;
        }

        public IOffer[] GetTraderOffers(string id)
        {
            return GetOffersAsArray(_allTraderOffers ,id);
        }

        public IOffer[] GetCompanyOffers(string id)
        {
            var sellOffers = GetOffersAsArray(_allSellCompanyOffers, id);
            var buyOffers = GetOffersAsArray(_allBuyCompanyOffers, id);

            IOffer[] res = new IOffer[sellOffers.Length + buyOffers.Length];
            uint cnt = 0;
            foreach (var offer in sellOffers)
            {
                res[cnt++] = offer;
            }
            foreach (var offer in buyOffers)
            {
                res[cnt++] = offer;
            }

            return res;
        }

        public IOffer MakeOffer(ITrader trader, ICompany company, decimal price, uint amount, bool isSellOffer)
        {
            IOffer offer = factory.NewOffer(company, trader, price, amount, isSellOffer);
            lock (_pendingOffers)
            {
                _pendingOffers.Add(offer);
            }

            return offer;
        }

        /**
         * Remove offer (when the work thread gets to it)
         */
        public void RemoveOffer(IOffer offer)
        {
            lock (_pendingDeleteOffers)
            {
                _pendingDeleteOffers.Add(offer);
            }
        }

        public void RemoveOffers(IEnumerable<IOffer> offers)
        {
            lock (_pendingDeleteOffers)
            {
                _pendingDeleteOffers.AddRange(offers);
            }
        }

        public bool HasPendingRequests()
        {
            if (_isDoingWork) return true;

            bool res = false;
            lock (_pendingDeleteOffers)
            {
                res = res || _pendingDeleteOffers.Any();
            }

            if (res) return res;
            lock (_pendingOffers)
            {
                res = res || _pendingOffers.Any();
            }

            return res;
        }

        /**
         * removes offer
         * if offer doesn't exist, do nothing
         */
        private void DeleteOffer(IOffer offer)
        {
            lock (_pendingOffers)
            {
                if (_pendingOffers.Remove(offer))
                    return;
            }
            var traderOffers = GetOfferList(_allTraderOffers, offer.Trader.Id);
            var companyOffers = offer.IsSellOffer ? 
                GetOfferSet(_allSellCompanyOffers, offer.Company.Id) :
                GetOfferSet(_allBuyCompanyOffers, offer.Company.Id);

            if (!companyOffers.Contains(offer)) return;

            traderOffers.Remove(offer);
            companyOffers.Remove(offer);
        }

        /**
         * makes offer and tries to make sale
         * returns the offer or null if made sale
         * removes all sale offers of said trader and company if this is a buy offer, and vice versa
         */
        private void AddOffer(IOffer offer)
        {
            if (!IsLegalOffer(offer)) return;

            List<IOffer> traderOffers = GetOfferList(_allTraderOffers, offer.Trader.Id);
            var companyOffers = offer.IsSellOffer ?
                GetOfferSet(_allBuyCompanyOffers, offer.Company.Id) :
                GetOfferSet(_allSellCompanyOffers, offer.Company.Id);
            var whereToAdd = offer.IsSellOffer ?
                GetOfferSet(_allSellCompanyOffers, offer.Company.Id) :
                GetOfferSet(_allBuyCompanyOffers, offer.Company.Id);

            DeleteConflictingOffers(offer, traderOffers, companyOffers);

            DeleteBadOffers(offer, companyOffers);

            IOffer bestExistingOffer;
            while (IsLegalOffer(bestExistingOffer = FindFittingOffer(companyOffers, offer.Price, offer.IsSellOffer))
                   && offer.Amount > 0)
            {
                offer = MakeSale(offer, bestExistingOffer, companyOffers);
            }

            if (offer.Amount == 0) return;

            if (bestExistingOffer == null)
            {
                traderOffers.Add(offer);
                whereToAdd.Add(offer);
                return;
            }
        }

        /**
         * @return new IOffer with res.Amount = prevOffer.Amount - amtToRemove
         */
        private IOffer OfferWithLessAmt(IOffer prevOffer, uint amtToRemove)
        {
            return factory.NewOffer(prevOffer.Company, prevOffer.Trader, prevOffer.Price,
                prevOffer.Amount - amtToRemove, prevOffer.IsSellOffer);
        }

        /*
         * @return offer changed
         */
        private IOffer MakeSale(IOffer offer, IOffer bestExistingOffer, SortedSet<IOffer> companyOffers)
        {
            uint amt = Math.Min(bestExistingOffer.Amount, offer.Amount);
            var traderOffers = GetOfferList(_allTraderOffers, bestExistingOffer.Trader.Id);


            traderOffers.Remove(bestExistingOffer);
            companyOffers.Remove(bestExistingOffer);

            bestExistingOffer = OfferWithLessAmt(bestExistingOffer, amt);
            offer = OfferWithLessAmt(offer, amt);

            if (bestExistingOffer.Amount > 0)
            {
                traderOffers.Add(bestExistingOffer);
                companyOffers.Add(bestExistingOffer);
            }

            string seller = offer.IsSellOffer ? offer.Trader.Id : bestExistingOffer.Trader.Id;
            string buyer = !offer.IsSellOffer ? offer.Trader.Id : bestExistingOffer.Trader.Id;
            Sale newSale = new Sale(seller, buyer, offer.Company.Id, bestExistingOffer.Price, amt);

            GetTraderFromId(bestExistingOffer.Trader.Id).MakeSale(newSale);
            GetTraderFromId(offer.Trader.Id).MakeSale(newSale);
            GetCompanyFromId(offer.Company.Id).AddSale(newSale);

            return offer;
        }

        /**
         * when making a sell offer, deletes buy offers of the same trader and vice versa
         */
        private static void DeleteConflictingOffers(IOffer offer, List<IOffer> traderOffers, SortedSet<IOffer> companyOffers)
        {
            traderOffers.RemoveAll(other => other.IsSellOffer != offer.IsSellOffer && offer.Company.Id == other.Company.Id);
            companyOffers.RemoveWhere(other => other.IsSellOffer != offer.IsSellOffer && offer.Trader.Id == other.Trader.Id);
        }

        /**
         * deletes illegal or null offers
         */
        private void DeleteBadOffers(IOffer offer, SortedSet<IOffer> companyOffers)
        {
            static bool IsBad(IOffer offer) => !IsLegalOffer(offer);

            var badOffers = companyOffers.Where(IsBad);
            foreach (var badOffer in badOffers)
            {
                GetOfferList(_allTraderOffers, badOffer.Trader.Id).Remove(badOffer);
            }

            companyOffers.RemoveWhere(IsBad);
        }

        /**
         * finds the best offers for the given offer
         * if isSellOffer then looks for buy offers and vice versa
         * if there's no such offer, returns null offer
         */
        private static IOffer FindFittingOffer(SortedSet<IOffer> companyOffers, decimal price, bool isSellOffer)
        {
            if (companyOffers.Count == 0) return null;

            IOffer bestOffer = isSellOffer ? companyOffers.Max : companyOffers.Min;

            if (!IsLegalOffer(bestOffer)) return null;

            if (bestOffer.Price == price) return bestOffer;

            //if best price is higher than the price I'm willing to pay or is lower than the price I want to get
            if (bestOffer.Price > price ^ isSellOffer) return null;

            return bestOffer;
        }

        public void CreateCompany(Dictionary<string, object> jsonDictionary)
        {
            string id = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["id"]);
            string name = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["name"]);
            decimal price = JsonSerializer.Deserialize<decimal>((JsonElement)jsonDictionary["currentPrice"]);
            uint amount = JsonSerializer.Deserialize<uint>((JsonElement)jsonDictionary["amount"]);

            ICompany company = factory.NewCompany(id, name, price);

            lock (_companies)
            {
                _companies.Add(company.Id, company);
            }

            ITrader trader = factory.NewCompanyTrader(company, amount);
            CreateTrader(trader);
            MakeOffer(trader, company, price, amount, true);
        }

        public void CreateTrader(Dictionary<string, object> jsonDictionary)
        {
            string id = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["id"]);
            string name = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["name"]);
            decimal money = JsonSerializer.Deserialize<decimal>((JsonElement)jsonDictionary["money"]);

            ITrader trader = factory.NewTrader(id, name, money);

            lock (_traders)
            {
                _traders.Add(trader.Id, trader);
            }
        }

        public void CreateTrader(ITrader trader)
        {
            lock (_traders)
            {
                _traders.Add(trader.Id, trader);
            }
        }

        public ICompany GetCompanyFromId(string id)
        {
            return _companies.GetValueOrDefault(id);
        }
        public ITrader GetTraderFromId(string id)
        {
            return _traders.GetValueOrDefault(id);
        }

        public TraderInfo GetTraderInfo(string id)
        {
            ITrader trader = GetTraderFromId(id);
            if (trader == null) throw new ArgumentException("StockMarket.GetTraderInfo() id doesn't fit any trader");
            return new TraderInfo(trader.Id, trader.Name, trader.Money, trader.GetPortfolio(), GetTraderOffers(id));
        }

        public CompanyInfo GetCompanyInfo(string id)
        {
            ICompany company = GetCompanyFromId(id);
            if (company == null)
                throw new ArgumentException("StockMarket.GetCompanyInfo() id doesn't fit any company");
            return new CompanyInfo(company.Id, company.Name, company.Price, GetCompanyOffers(id),
                company.GetRecentSales());
        }

        /**
        * if sell offer checks the seller has the stock
        * if buy offer checks the buyer has the money
        */
        private static bool IsLegalOffer(IOffer offer)
        {
            if (offer == null) return false;
            if (offer.IsSellOffer && offer.Trader.StockAmount(offer.Company.Id) < offer.Amount) return false;
            if (!offer.IsSellOffer && offer.Trader.Money < offer.Price) return false;
            return true;
        }

        /**
         * locks and gets list and clears it
         * does action on all list
         */
        private void DoWork<T>(List<T> lst, Action<T> action)
        {
            List<T> copy;
            lock (lst)
            {
                _isDoingWork = lst.Count > 0;

                copy = lst.ToList();
                lst.Clear();
            }
            copy.ForEach(action);
            _isDoingWork = false;
        }



        private void WorkThread()
        {
            while (_run)
            {
                DoWork(_pendingOffers, AddOffer);
                DoWork(_pendingDeleteOffers, DeleteOffer);
                Thread.Sleep(1);
            }
        }

        /**
         * I honestly don't think it matters if there's race conditions here
         */
        private void ChangeCompanyPrices()
        {
            const int sleepTime = 20000; //MAGIC NUMBER
            while (_run)
            {
                Thread.Sleep(sleepTime);
                foreach (var company in _companies.Values)
                {
                    company.RandomChangePrice();
                }
            }
        }

        public bool Init()
        {
            lock (_lock)
            {
                if (_run) return false;

                _run = true;
                Thread work = new Thread(WorkThread);
                Thread priceChange = new Thread(ChangeCompanyPrices);

                work.Start();
                priceChange.Start();

                return true;
            }
        }
    }
}
