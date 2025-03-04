using System;
using System.Collections.Generic;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Utility;

namespace Simulabs_Burse_Console.Trader;

public abstract class SellingTrader : ITrader
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract decimal Money { get; protected set; }

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
    public abstract void MakeSale(Sale sale);

    /**
     * throws exception if doesn't have said stock
     * assumes price >= 0
     */
    protected virtual void SellStocks(string companyId, decimal price, uint amt)
    {
        if (StockAmount(companyId) < amt)
            throw new ArgumentException("ITrader.SellStocks() doesn't have the stock to sell");

        _portfolio[companyId] -= amt;
    }
}