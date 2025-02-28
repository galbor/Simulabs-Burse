using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Simulabs_Burse_Console;

public class OfferComparerByPrice : IComparer<IOffer>
{
    public int Compare(IOffer first, IOffer second)
    {
        if (first == null) return -1;
        if (second == null) return 1;
        if (Equals(second)) return 0;

        if (first.Price == second.Price) return second.OfferId - first.OfferId;

        if (first.Price > second.Price) return 1;
        return -1;

    }
}