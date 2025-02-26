using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console.Factories
{
    public class RegularStockMarketFactory : IStockMarketFactory
    {
        public RegularStockMarketFactory() { }

        public ICompany NewCompany(string id, string name, decimal price)
        {
            return new Company10History(id, name, price);
        }

        public IOffer NewOffer(ICompany company, ITrader trader, decimal price, uint amount, bool isSellOffer)
        {
            return new RegularOffer(company, trader, price, amount, isSellOffer);
        }

        public ITrader NewTrader(string id, string name, decimal money)
        {
            return new Trader8History(id, name, money);
        }

        public ITrader NewCompanyTrader(ICompany company, uint amount)
        {
            return new CompanyTrader(company, amount);
        }
    }
}
