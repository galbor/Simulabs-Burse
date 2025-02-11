using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console
{
    internal static class StockMarket
    {
        private static readonly Dictionary<string, Company> _companies = new Dictionary<string, Company>();
        private static readonly Dictionary<string, Trader> _traders = new Dictionary<string, Trader>() { { Trader.EmptyTrader._id, Trader.EmptyTrader } };

        private static readonly Dictionary<string, List<Offer>> _allTraderOffers = new Dictionary<string, List<Offer>>(); //traderId -> list(offer)
        private static readonly Dictionary<string, SortedSet<Offer>> _allSellCompanyOffers = new Dictionary<string, SortedSet<Offer>>(); //companyId -> list(offer)
        private static readonly Dictionary<string, SortedSet<Offer>> _allBuyCompanyOffers = new Dictionary<string, SortedSet<Offer>>(); //companyId -> list(offer)

        private static readonly List<Offer> _pendingOffers = new List<Offer>();
        private static readonly List<Offer> _pendingDeleteOffers = new List<Offer>();

        private static bool _run = false;

        private static object _lock = new Object();
        private static bool _isDoingWork = false;


        public static List<Company> GetAllCompanies()
        {
            return _companies.Values.ToList();
        }

        public static List<Trader> GetAllTraders()
        {
            return _traders.Values.ToList();
        }

        /**
         * gets the offers list of an id
         * if doesn't exist in dictionary, creates an empty list
         */
        private static List<Offer> GetOfferList(Dictionary<string, List<Offer>> offers, string id)
        {
            if (!offers.TryGetValue(id, out List<Offer> res))
            {
                res = new List<Offer>();
                offers.Add(id, res);
            }

            return res;
        }

        private static SortedSet<Offer> GetOfferSet(Dictionary<string, SortedSet<Offer>> offers, string id)
        {
            if (!offers.TryGetValue(id, out SortedSet<Offer> res))
            {
                res = new SortedSet<Offer>();
                offers.Add(id, res);
            }

            return res;
        }

        private static Offer[] GetOffersAsArray(Dictionary<string, List<Offer>> offersDict, string id)
        {
            var offers = GetOfferList(offersDict, id);
            Offer[] res = new Offer[offers.Count];
            offers.CopyTo(res);
            res = res.Where(offer => offer.IsLegal()).ToArray();
            return res;
        }

        private static Offer[] GetOffersAsArray(Dictionary<string, SortedSet<Offer>> offersDict, string id)
        {
            var offers = GetOfferSet(offersDict, id);

            Offer[] res = new Offer[offers.Count];
            offers.CopyTo(res);
            res = res.Where(offer => offer != null && offer.IsLegal()).ToArray();
            return res;
        }

        /**
         * returns different list so the real list can't be edited
         */
        public static Offer[] GetTraderOffers(string id)
        {
            return GetOffersAsArray(_allTraderOffers ,id);
        }

        /**
         * returns different list so the real list can't be edited
         */
        public static Offer[] GetCompanyOffers(string id)
        {
            var sellOffers = GetOffersAsArray(_allSellCompanyOffers, id);
            var buyOffers = GetOffersAsArray(_allBuyCompanyOffers, id);

            Offer[] res = new Offer[sellOffers.Length + buyOffers.Length];
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

        /**
         * makes offer
         * returns fitting offer
         */
        public static Offer MakeOffer(Trader trader, Company company, decimal price, uint amount, bool isSellOffer)
        {
            Offer offer = new Offer(company._id, trader._id, price, amount, isSellOffer);
            lock (_pendingOffers)
            {
                _pendingOffers.Add(offer);
            }

            return offer;
        }

        /**
         * Remove offer (when the work thread gets to it)
         */
        public static void RemoveOffer(Offer offer)
        {
            lock (_pendingDeleteOffers)
            {
                _pendingDeleteOffers.Add(offer);
            }
        }

        public static void RemoveOffers(IEnumerable<Offer> offers)
        {
            lock (_pendingDeleteOffers)
            {
                _pendingDeleteOffers.AddRange(offers);
            }
        }

        public static bool HasPendingRequests()
        {
            if (_isDoingWork) return true;

            bool res = false;
            lock (_pendingDeleteOffers)
            {
                res = res || _pendingDeleteOffers.Any();
            }
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
        private static void DeleteOffer(Offer offer)
        {
            lock (_pendingOffers)
            {
                if (_pendingOffers.Remove(offer))
                    return;
            }
            var traderOffers = GetOfferList(_allTraderOffers, offer.TraderId);
            var companyOffers = offer.IsSellOffer ? 
                GetOfferSet(_allSellCompanyOffers, offer.CompanyId) :
                GetOfferSet(_allBuyCompanyOffers, offer.CompanyId);

            if (!companyOffers.Contains(offer)) return;

            traderOffers.Remove(offer);
            companyOffers.Remove(offer);
        }

        /**
         * makes offer and tries to make sale
         * returns the offer or null if made sale
         * removes all sale offers of said trader and company if this is a buy offer, and vice versa
         */
        private static void AddOffer(Offer offer)
        {
            if (!offer.IsLegal()) return;

            List<Offer> traderOffers = GetOfferList(_allTraderOffers, offer.TraderId);
            var companyOffers = offer.IsSellOffer ?
                GetOfferSet(_allBuyCompanyOffers, offer.CompanyId) :
                GetOfferSet(_allSellCompanyOffers, offer.CompanyId);
            var whereToAdd = offer.IsSellOffer ?
                GetOfferSet(_allSellCompanyOffers, offer.CompanyId) :
                GetOfferSet(_allBuyCompanyOffers, offer.CompanyId);

            DeleteConflictingOffers(offer, traderOffers, companyOffers);

            DeleteBadOffers(offer, companyOffers);

            Offer bestExistingOffer;
            while ((bestExistingOffer = FindFittingOffer(companyOffers, offer.Price, offer.IsSellOffer)).IsLegal()
                   && !bestExistingOffer.IsNull() && offer.Amount > 0)
            {
                MakeSale(offer, bestExistingOffer, companyOffers);
            }

            if (offer.Amount == 0) return;

            if (bestExistingOffer.IsNull())
            {
                traderOffers.Add(offer);
                whereToAdd.Add(offer);
                return;
            }
        }

        private static void MakeSale(Offer offer, Offer bestExistingOffer, SortedSet<Offer> companyOffers)
        {
            uint amt = Math.Min(bestExistingOffer.Amount, offer.Amount);

            if (bestExistingOffer.Amount == amt)
            {
                GetOfferList(_allTraderOffers, bestExistingOffer.TraderId).Remove(bestExistingOffer);
                companyOffers.Remove(bestExistingOffer);
            }

            bestExistingOffer.RemoveFromAmount(amt);
            offer.RemoveFromAmount(amt);

            string seller = offer.IsSellOffer ? offer.TraderId : bestExistingOffer.TraderId;
            string buyer = !offer.IsSellOffer ? offer.TraderId : bestExistingOffer.TraderId;
            Sale newSale = new Sale(seller, buyer, offer.CompanyId, bestExistingOffer.Price, amt);

            GetTraderFromId(bestExistingOffer.TraderId).MakeSale(newSale);
            GetTraderFromId(offer.TraderId).MakeSale(newSale);
            GetCompanyFromId(offer.CompanyId).AddSale(newSale);
        }

        /**
         * when making a sell offer, deletes buy offers of the same trader and vice versa
         */
        private static void DeleteConflictingOffers(Offer offer, List<Offer> traderOffers, SortedSet<Offer> companyOffers)
        {
            traderOffers.RemoveAll(other => other.IsSellOffer != offer.IsSellOffer && offer.CompanyId == other.CompanyId);
            companyOffers.RemoveWhere(other => other.IsSellOffer != offer.IsSellOffer && offer.TraderId == other.TraderId);
        }

        /**
         * deletes illegal or null offers
         */
        private static void DeleteBadOffers(Offer offer, SortedSet<Offer> companyOffers)
        {
            Predicate<Offer> isBad = offer => !offer.IsLegal() || offer.IsNull();

            var badOffers = companyOffers.Where(offer => isBad(offer));
            foreach (var badOffer in badOffers)
            {
                GetOfferList(_allTraderOffers, badOffer.TraderId).Remove(badOffer);
            }

            companyOffers.RemoveWhere(isBad);
        }

        /**
         * finds the best offers for the given offer
         * if isSellOffer then looks for buy offers and vice versa
         * if there's no such offer, returns null offer
         */
        private static Offer FindFittingOffer(SortedSet<Offer> companyOffers, decimal price, bool isSellOffer)
        {
            if (companyOffers.Count == 0) return Offer.NullOffer;

            Offer bestOffer = isSellOffer ? companyOffers.Max : companyOffers.Min;

            if (!bestOffer.IsLegal()) return Offer.NullOffer;

            if (bestOffer.Price == price) return bestOffer;

            //if best price is higher than the price I'm willing to pay or is lower than the price I want to get
            if (bestOffer.Price > price ^ isSellOffer) return Offer.NullOffer;

            return bestOffer;
        }

        public static void CreateCompany(Dictionary<string, object> jsonDictionary)
        {
            string id = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["id"]);
            string name = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["name"]);
            decimal price = JsonSerializer.Deserialize<decimal>((JsonElement)jsonDictionary["currentPrice"]);
            uint amount = JsonSerializer.Deserialize<uint>((JsonElement)jsonDictionary["amount"]);

            Company company = new Company(id.ToString(), name.ToString(), price);

            lock (_companies)
            {
                _companies.Add(company._id, company);
            }

            Trader.AddToEmptyTraderPortfolio(company, amount);
            MakeOffer(Trader.EmptyTrader, company, price, amount, true);
        }

        public static void CreateTrader(Dictionary<string, object> jsonDictionary)
        {
            string id = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["id"]);
            string name = JsonSerializer.Deserialize<string>((JsonElement)jsonDictionary["name"]);
            decimal money = JsonSerializer.Deserialize<decimal>((JsonElement)jsonDictionary["money"]);

            Trader trader = new Trader(id, name, money);

            lock (_traders)
            {
                _traders.Add(trader._id, trader);
            }
        }

        /**
         * returns null if no such company exists
         */
        public static Company GetCompanyFromId(string id)
        {
            if (_companies.TryGetValue(id, out Company company))
                return company;
            return null;
        }
        /**
         * returns null if no such trader exists
         */
        public static Trader GetTraderFromId(string id)
        {
            if (_traders.TryGetValue(id, out Trader trader))
                return trader;
            return null;
        }

        /**
         * locks and gets list and clears it
         * does action on all list
         */
        private static void DoWork<T>(List<T> lst, Action<T> action)
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



        private static void WorkThread()
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
        private static void ChangeCompanyPrices()
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

        /**
         * starts threads if they're not running
         * @return true iff started threads
         */
        public static bool StartThreads()
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
