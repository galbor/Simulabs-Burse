namespace Simulabs_Burse_Console.Offer.LegalOfferChecker;

public class RegularLegalOfferChecker : ILegalOfferChecker
{
    /**
        * if sell offer checks the seller has the stock
        * if buy offer checks the buyer has the money
        */
    public bool IsLegalOffer(IOffer offer)
    {
        if (offer == null) return false;
        if (offer.IsSellOffer && offer.Trader.StockAmount(offer.Company.Id) < offer.Amount) return false;
        if (!offer.IsSellOffer && offer.Trader.Money < offer.Price) return false;
        return true;
    }
}