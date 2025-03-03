using System;
using System.Collections.Generic;
using Simulabs_Burse_Console.Company;
using Simulabs_Burse_Console.POD;

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

    public CompanyTrader(ICompany company, uint amount)
    {
        Id = company.Name + company.Id;
        Name = company.Name;
        _portfolio[company.Id] = amount;
    }

    public override void MakeSale(Sale sale)
    {
        if (sale.SellerId != Id)
            throw new ArgumentException("CompanyTrader.MakeSale() can't make sale it's not the seller in");
        SellStocks(sale.CompanyId, sale.Price, sale.Amount);
    }

    public override Sale[] GetRecentSales()
    {
        return [];
    }
}