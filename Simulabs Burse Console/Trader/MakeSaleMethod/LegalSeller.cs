using System.ComponentModel.Design;
using System;
using Simulabs_Burse_Console.POD;
using System.Diagnostics;

namespace Simulabs_Burse_Console.Trader.MakeSaleMethod;

public class LegalSeller : ISeller
{
    public void MakeSale(string thisId, Sale sale, ref decimal money, ref uint stockAmt)
    {
        if (sale.SellerId == thisId) SellStocks(sale, ref money, ref stockAmt);
        else if (sale.BuyerId == thisId) BuyStocks(sale, ref money, ref stockAmt);
        else throw new ArgumentException("Trader8History can't make sale he's not included in");
    }

    private void SellStocks(Sale sale, ref decimal money, ref uint stockAmt)
    {
        if (stockAmt < sale.Amount)
            throw new ArgumentException("LegalSeller.SellStocks() doesn't have the stock to sell");

        stockAmt -= sale.Amount;
        money += sale.Price * sale.Amount;
    }

    private void BuyStocks(Sale sale, ref decimal money, ref uint stockAmt)
    {
        if (sale.Price * sale.Amount > money) throw new ArgumentException("LegalSeller.BuyStocks() too pricey");
        money -= sale.Price * sale.Amount;

        stockAmt += sale.Amount;
    }
}