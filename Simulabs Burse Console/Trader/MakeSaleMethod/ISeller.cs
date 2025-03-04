using Simulabs_Burse_Console.POD;

namespace Simulabs_Burse_Console.Trader.MakeSaleMethod;

public interface ISeller
{
    public void MakeSale(string thisId, Sale sale, ref decimal money, ref uint stockAmt);
}