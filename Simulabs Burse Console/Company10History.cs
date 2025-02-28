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
        private readonly LimitedQueue<Sale> _recentSaleHistory;

        public Company10History(string id, string name)
        {
            Id = id;
            Name = name;
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

        public void AddSale(Sale sale)
        {
            if (sale.CompanyId != Id) throw new ArgumentException("ICompany.AddSale() added sale with wrong company");

            _recentSaleHistory.Enqueue(sale);
        }
    }
}
