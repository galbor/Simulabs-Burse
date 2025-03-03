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
        public uint Amount { get;}
        public bool IsSellOffer { get; }
        public int OfferId {get;}

        public RegularOffer(IIdGenerator<int> idGenerator, ICompany company, ITrader trader, decimal price, uint amount, bool isSellOffer)
        {
            this.Company = company;
            this.Trader = trader;
            this.Price = price;
            this.Amount = amount;
            this.IsSellOffer = isSellOffer;
            this.OfferId = idGenerator.Next();
        }
    }
}
