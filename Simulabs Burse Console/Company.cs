using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console
{
    internal class Company
    {
        public readonly string _id;
        public readonly string _name;
        public decimal Price { get; private set; }
        private readonly LimitedQueue<Sale> _recentSaleHistory;

        public Company(string id, string name, decimal price)
        {
            _id = id;
            _name = name;
            Price = price;
            _recentSaleHistory = new LimitedQueue<Sale>(10); //MAGIC NUMBER specified in word document
        }

        /**
         * returns 10 recent sales
         * with arr[0] being the oldest and arr[^1] being the newest
         */
        public Sale[] GetRecentSales()
        {
            return _recentSaleHistory.AsArray();
        }

        /**
         * randomly changes prices
         * using a somewhat normal distribution (without negatives)
         */
        public void RandomChangePrice()
        {
            decimal stdDev = Price / 10; //MAGIC NUMBER
            decimal distanceFromZero = 0.5M; //MAGIC NUMBER
            Price = Math.Abs(MyUtils.NormalDistribution(Price, stdDev)-distanceFromZero) + distanceFromZero;
        }

        /**
         * adds sale to history
         * throws exception if sale is of wrong company
         */
        public void AddSale(Sale sale)
        {
            if (sale.CompanyId != _id) throw new ArgumentException("Company.AddSale() added sale with wrong company");

            _recentSaleHistory.Enqueue(sale);
            Price = sale.Price;
        }


        public struct CompanyInfo
        {
            public readonly string Id;
            public readonly string Name;
            public readonly decimal Price;
            public readonly Offer[] Offers;
            public readonly Sale[] RecentSales;

            public CompanyInfo(Company company)
            {
                Id = company._id;
                Name = company._name;
                Price = company.Price;
                Offers = StockMarket.GetCompanyOffers(company._id);
                RecentSales = company.GetRecentSales();
            }

            public override string ToString()
            {
                StringBuilder res = new StringBuilder();
                res.AppendFormat("ID: {0}\n", Id);
                res.AppendFormat("Name: {0}\n", Name);
                res.AppendFormat("Price: {0}\n", Price);
                res.Append("Offers:\n");
                foreach (var offer in Offers)
                {
                    if (offer.IsSellOffer)
                        res.Append("    Sell: ");
                    else res.Append("   Buy: ");
                    res.AppendFormat("Trader {0}, Price {1}, Amount {2}\n", offer.TraderId, offer.Price, offer.Amount);
                }

                res.Append("Recent sale history:\n");

                foreach (var sale in RecentSales)
                {
                    res.AppendFormat("\t{0}\n", sale.ToString());
                }

                return res.ToString();
            }
        }
    }
}
