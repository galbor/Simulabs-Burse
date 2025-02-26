using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Utility;

namespace Simulabs_Burse_Console
{
    public class Company10History : ICompany
    {
        public string Id { get; }
        public string Name {get;}
        public decimal Price { get; private set; }
        private readonly LimitedQueue<Sale> _recentSaleHistory;

        public Company10History(string id, string name, decimal price)
        {
            Id = id;
            Name = name;
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


        public void AddSale(Sale sale)
        {
            if (sale.CompanyId != Id) throw new ArgumentException("ICompany.AddSale() added sale with wrong company");

            _recentSaleHistory.Enqueue(sale);
            Price = sale.Price;
        }
    }
}
