namespace Simulabs_Burse_Console.PriceChanger;

public interface INewPriceCalculator
{
    public decimal NewPrice(decimal prevPrice);
}