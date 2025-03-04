using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Trader.MakeSaleMethod;
using Simulabs_Burse_Console.Utility;

namespace Simulabs_Burse_Console.Trader
{
    public class Trader8History : SellingTrader
    {
        public override string Id { get; }
        public override string Name { get; }
        public override decimal Money { get; protected set; }
        private readonly LimitedQueue<Sale> _recentSaleHistory;

    public Trader8History(string id, string name, decimal money, ISeller seller)
        {
            Id = id;
            Name = name;
            Money = money;
            _recentSaleHistory = new LimitedQueue<Sale>(8); //MAGIC NUMBER specified in word document
            Seller = seller;
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
            base.MakeSale(sale);
            _recentSaleHistory.Enqueue(sale);
        }
    }
}
