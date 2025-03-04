using System;
using System.Collections.Generic;
using Simulabs_Burse_Console.Company;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Trader.MakeSaleMethod;

namespace Simulabs_Burse_Console.Trader;

public class CompanyTrader : SellingTrader
{
    public override string Id { get; }
    public override string Name { get; }
    public override decimal Money
    {
        get => 0M;
        protected set { }
    }

    public CompanyTrader(ICompany company, uint amount, ISeller seller)
    {
        Id = company.Name + company.Id;
        Name = company.Name;
        _portfolio[company.Id] = amount;
        Seller = seller;
    }

    public override Sale[] GetRecentSales()
    {
        return [];
    }
}