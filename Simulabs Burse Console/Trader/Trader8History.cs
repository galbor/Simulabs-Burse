using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Utility;

namespace Simulabs_Burse_Console.Trader
{
    public class Trader8History : SellingTrader
    {
        public override string Id { get; }
        public override string Name { get; }
        public override decimal Money { get; protected set; }
        private readonly LimitedQueue<Sale> _recentSaleHistory;

        public Trader8History(string id, string name, decimal money)
        {
            Id = id;
            Name = name;
            Money = money;
            _recentSaleHistory = new LimitedQueue<Sale>(8); //MAGIC NUMBER specified in word document
        }

        /**
        * returns 8 recent sales
        * with arr[0] being the oldest and arr[^1] being the newest
        */
        public override Sale[] GetRecentSales()
        {
            return _recentSaleHistory.AsArray();
        }

        public override void MakeSale(Sale sale)
        {
            if (sale.SellerId == Id) SellStocks(sale.CompanyId, sale.Price, sale.Amount);
            else if (sale.BuyerId == Id) BuyStocks(sale.CompanyId, sale.Price, sale.Amount);
            else throw new ArgumentException("Trader8History can't make sale he's not included in");

            _recentSaleHistory.Enqueue(sale);
        }

        /**
         * throws exception if price*amt > Money
         */
        private void BuyStocks(string companyId, decimal price, uint amt)
        {
            if (price * amt > Money) throw new ArgumentException("Trader8History.BuyStocks() too pricey");
            Money -= price * amt;

            if (StockAmount(companyId) > 0) _portfolio[companyId] += amt;
            else _portfolio[companyId] = amt;
        }

        protected override void SellStocks(string companyId, decimal price, uint amt)
        {
            base.SellStocks(companyId, price, amt);
            Money += price * amt;
        }
    }
}
