using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console
{
    public class RegularOffer : IOffer
    {
        public ICompany Company { get; }
        public ITrader Trader {get;}
        public decimal Price {get;}
        public uint Amount { get; private set; }
        public bool IsSellOffer { get; }
        public int OfferId {get;}

        public RegularOffer(ICompany company, ITrader trader, decimal price, uint amount, bool isSellOffer)
        {
            this.Company = company;
            this.Trader = trader;
            this.Price = price;
            this.Amount = amount;
            this.IsSellOffer = isSellOffer;
            this.OfferId = IOffer.GenerateOfferId();
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
            if (IsSellOffer && Trader.StockAmount(Company.Id) < Amount) return false;
            if (!IsSellOffer && Trader.Money < Price) return false;
            return true;
        }


        public int CompareTo(object other)
        {
            if (other == null) return 1;
            if (Equals(other)) return 0;
            if (other is not IOffer) return other.GetHashCode() - GetHashCode();

            IOffer otheroffer = (IOffer)other;

            if (Price == otheroffer.Price) return otheroffer.OfferId - OfferId;

            if (Price > otheroffer.Price) return 1;
            return -1;

        }
    }
}
