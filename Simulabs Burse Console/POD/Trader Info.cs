using System.Collections.Generic;
using System.Text;
using Simulabs_Burse_Console.Offer;

namespace Simulabs_Burse_Console.POD;

public class TraderInfo(string id, string name, decimal money, KeyValuePair<string, uint>[] portfolio, IOffer[] offers)
{
    public readonly string Id = id;
    public readonly string Name = name;
    public readonly decimal Money = money;
    public readonly KeyValuePair<string, uint>[] Portfolio = portfolio;
    public readonly IOffer[] Offers = offers;

    public override string ToString()
    {
        StringBuilder res = new StringBuilder();
        res.AppendFormat("ID: {0}\n", Id);
        res.AppendFormat("Name: {0}\n", Name);
        res.AppendFormat("Balance: {0}\n", Money);
        res.Append("Offers:\n");
        foreach (var offer in Offers)
        {
            res.AppendFormat("{0}.", offer.OfferId);
            if (offer.IsSellOffer)
                res.Append("\tSell: ");
            else res.Append("\tBuy: ");
            res.AppendFormat("ICompany {0}, Price {1}, Amount {2}\n", offer.Company.Id, offer.Price, offer.Amount);
        }

        res.Append("Portfolio:\n");

        foreach (var pair in Portfolio)
        {
            res.AppendFormat("ICompany {0}, amount {1}\n", pair.Key, pair.Value);
        }

        return res.ToString();
    }
}