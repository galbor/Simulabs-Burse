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
}