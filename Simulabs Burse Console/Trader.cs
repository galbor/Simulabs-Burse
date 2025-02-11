using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console
{
    internal class Trader
    {
        public readonly string _id;
        public readonly string _name;
        public decimal Money { get; private set; }
        private readonly LimitedQueue<Sale> _recentSaleHistory;
        private readonly Dictionary<string, uint> _portfolio;

        private static readonly Trader emptyTrader = new Trader("","",0);

        public static Trader EmptyTrader { get => emptyTrader; }

        public Trader(string id, string name, decimal money)
        {
            _id = id;
            _name = name;
            Money = money;
            _recentSaleHistory = new LimitedQueue<Sale>(8); //MAGIC NUMBER specified in word document
            _portfolio = new Dictionary<string, uint>(); //companyId -> amount of stocks
        }

        /**
        * returns 8 recent sales
        * with arr[0] being the oldest and arr[^1] being the newest
        */
        public Sale[] GetRecentSales() {
            return _recentSaleHistory.AsArray();
        }

        public void MakeSale(Sale sale)
        {
            if (sale.SellerId == _id) SellStocks(sale.CompanyId, sale.Price, sale.Amount);
            else if (sale.BuyerId == _id) BuyStocks(sale.CompanyId, sale.Price, sale.Amount);
            else throw new ArgumentException("Trader can't make sale he's not included in");

            _recentSaleHistory.Enqueue(sale);
        }

        public uint StockAmount(string id)
        {
            if (_portfolio.TryGetValue(id, out uint res)) return res;
            return 0;
        }

        public bool HasStock(string id)
        {
            return _portfolio.ContainsKey(id);
        }

        /**
         * throws exception if doesn't have said stock
         * assumes price >= 0
         */
        private void SellStocks(string companyId, decimal price, uint amt)
        {
            if (!_portfolio.TryGetValue(companyId, out uint prevamount) || prevamount < amt)
                throw new ArgumentException("Trader.SellStocks() doesn't have the stock to sell");

            _portfolio[companyId]-= amt;
            Money += price*amt;
        }

        /**
         * throws exception if price*amt > Money
         */
        private void BuyStocks(string companyId, decimal price, uint amt)
        {
            if (price*amt > Money) throw new ArgumentException("Trader.BuyStocks() too pricey");
            Money -= price*amt;

            if (_portfolio.ContainsKey(companyId)) _portfolio[companyId] += amt;
            else _portfolio[companyId] = amt;
        }



        public static void AddToEmptyTraderPortfolio(Company company, uint amount)
        {
            if (EmptyTrader._portfolio.ContainsKey(company._id)) EmptyTrader._portfolio[company._id] += amount;
            else EmptyTrader._portfolio[company._id] = amount;
        }


        public struct TraderInfo
        {
            public readonly string Id;
            public readonly string Name;
            public readonly decimal Money;
            public readonly KeyValuePair<string, uint>[] Portfolio;
            public readonly Offer[] Offers;

            public TraderInfo(Trader trader)
            {
                Id = trader._id;
                Name = trader._name;
                Money = trader.Money;
                Offers = StockMarket.GetTraderOffers(trader._id);
                Portfolio = new KeyValuePair<string, uint>[trader._portfolio.Count];

                uint cnt = 0;
                foreach (var pair in trader._portfolio)
                {
                    Portfolio[cnt++] = pair;
                }
            }

            public override string ToString()
            {
                StringBuilder res = new StringBuilder();
                res.AppendFormat("ID: {0}\n", Id);
                res.AppendFormat("Name: {0}\n", Name);
                res.AppendFormat("Balance: {0}\n", Money);
                res.Append("Offers:\n");
                foreach (var offer in Offers)
                {
                    res.AppendFormat("{0}.", offer.OfferId);
                    if (offer.IsSellOffer)
                        res.Append("\tSell: ");
                    else res.Append("\tBuy: ");
                    res.AppendFormat("Company {0}, Price {1}, Amount {2}\n", offer.CompanyId, offer.Price, offer.Amount);
                }

                res.Append("Portfolio:\n");

                foreach (var pair in Portfolio)
                {
                    res.AppendFormat("Company {0}, amount {1}\n", pair.Key, pair.Value);
                }

                return res.ToString();
            }
        }
    }
}
