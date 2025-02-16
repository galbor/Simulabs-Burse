using System;
using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console;

public interface IOffer : IComparable
{
    public ICompany Company { get; }
    public ITrader Trader {get;}
    public decimal Price {get;}
    public uint Amount { get; }
    public bool IsSellOffer { get; }
    public int OfferId { get; }

    private static int _cnt = 0;
    private static Object _lock = new object();

    protected static int GenerateOfferId()
    {
        lock (_lock)
            return _cnt++;
    }

    /**
         * does Amount -= toRemove
         * doesn't do it if toRemove > Amount
         * @return true if successful
         */
    public bool RemoveFromAmount(uint toRemove);

    /**
         * if sell offer checks the seller has the stock
         * if buy offer checks the buyer has the money
         */
    public bool IsLegal();
}