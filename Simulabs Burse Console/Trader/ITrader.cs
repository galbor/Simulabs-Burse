using System.Collections.Generic;
using Simulabs_Burse_Console.POD;

namespace Simulabs_Burse_Console.Trader;

public interface ITrader
{
    public string Id { get; }
    public string Name { get; }
    public decimal Money { get; }

    /**
        * returns recent sales
        * with arr[0] being the oldest and arr[^1] being the newest
        */
    public Sale[] GetRecentSales();

    /**
     * returns array of pairs of company ID (string) and amount of stock (uint)
     */
    public KeyValuePair<string, uint>[] GetPortfolio();

    /**
    * should be called when this trader is included in a sale
    * should throw exception
    */
    public void MakeSale(Sale sale);

    public uint StockAmount(string id);
}