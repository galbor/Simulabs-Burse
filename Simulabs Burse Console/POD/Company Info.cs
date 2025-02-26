using System.Text;

namespace Simulabs_Burse_Console.POD;

public readonly struct CompanyInfo (string id, string name, decimal price, IOffer[] offers, Sale[] recentSales)
{
    public readonly string Id = id;
    public readonly string Name = name;
    public readonly decimal Price = price;
    public readonly IOffer[] Offers = offers;
    public readonly Sale[] RecentSales = recentSales;

    public override string ToString()
    {
        StringBuilder res = new StringBuilder();
        res.AppendFormat("ID: {0}\n", Id);
        res.AppendFormat("Name: {0}\n", Name);
        res.AppendFormat("Price: {0}\n", Price);
        res.Append("Offers:\n");
        foreach (var offer in Offers)
        {
            if (offer.IsSellOffer)
                res.Append("    Sell: ");
            else res.Append("   Buy: ");
            res.AppendFormat("ITrader {0}, Price {1}, Amount {2}\n", offer.Trader.Id, offer.Price, offer.Amount);
        }

        res.Append("Recent sale history:\n");

        foreach (var sale in RecentSales)
        {
            res.AppendFormat("\t{0}\n", sale.ToString());
        }

        return res.ToString();
    }
}