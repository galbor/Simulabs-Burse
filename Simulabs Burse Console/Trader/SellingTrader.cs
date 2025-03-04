using System;
using System.Collections.Generic;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Trader.MakeSaleMethod;
using Simulabs_Burse_Console.Utility;

namespace Simulabs_Burse_Console.Trader;

public abstract class SellingTrader : ITrader
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract decimal Money { get; protected set; }

    public ISeller Seller { get; set; }

    protected readonly Dictionary<string, uint> _portfolio = new Dictionary<string, uint>();

    public KeyValuePair<string, uint>[] GetPortfolio()
    {
        return MyUtils.GetDictAsKeyValuePairArr(_portfolio);
    }

    public uint StockAmount(string id)
    {
        if (_portfolio.TryGetValue(id, out uint res)) return res;
        return 0;
    }

    public abstract Sale[] GetRecentSales();

    public virtual void MakeSale(Sale sale)
    {
        decimal money = Money;
        uint stockAmt = StockAmount(sale.CompanyId);

        Seller.MakeSale(Id, sale, ref money, ref stockAmt);

        Money = money;
        _portfolio[sale.CompanyId] = stockAmt;
    }
}