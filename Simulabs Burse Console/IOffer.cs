using System;
using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console;

public interface IOffer
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
}