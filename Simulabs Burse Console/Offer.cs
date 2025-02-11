using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console
{
    internal class Offer : IComparable
    {
        public readonly string CompanyId;
        public readonly string TraderId;
        public readonly decimal Price;
        public uint Amount { get; private set; }
        public readonly bool IsSellOffer;
        public readonly int OfferId;

        private static int _cnt = 0;
        private static Object _lock = new object();

        public Offer(string companyId, string traderId, decimal price, uint amount, bool isSellOffer)
        {
            this.CompanyId = companyId;
            this.TraderId = traderId;
            this.Price = price;
            this.Amount = amount;
            this.IsSellOffer = isSellOffer;
            lock (_lock)
                this.OfferId = _cnt++;
        }

        /**
         * does Amount -= toRemove
         * doesn't do it if toRemove > Amount
         * @return true if successful
         */
        public bool RemoveFromAmount(uint toRemove)
        {
            if (toRemove > Amount) return false;
            Amount -= toRemove;
            return true;
        }

        /**
         * if sell offer checks the seller has the stock
         * if buy offer checks the buyer has the money
         */
        public bool IsLegal()
        {
            if (IsNull()) return false;
            Trader trader = StockMarket.GetTraderFromId(TraderId);
            if (IsSellOffer && trader.StockAmount(CompanyId) < Amount) return false;
            if (!IsSellOffer && trader.Money < Price) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Offer))
            {
                return ((Offer)obj).OfferId == OfferId;
            }

            return base.Equals(obj);
        }

        public int CompareTo(object other)
        {
            if (Equals(other)) return 0;
            if (other.GetType() != typeof(Offer)) return other.GetHashCode() - GetHashCode();

            Offer otheroffer = (Offer)other;

            if (Price == otheroffer.Price) return otheroffer.OfferId - OfferId;

            if (Price > otheroffer.Price) return 1;
            return -1;

        }

        public override int GetHashCode()
        {
            return OfferId;
        }

        public static bool IsNull(Offer offer)
        {
            return offer.Price < 0 || offer.Amount == 0;
        }

        public bool IsNull()
        {
            return Offer.IsNull(this);
        }

        public static Offer NullOffer
        {
            get => new Offer("", "", -1, 0, true);
        }
    }
}
