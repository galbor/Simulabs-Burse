using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualBasic;
using Simulabs_Burse_Console.Stock_Market;

namespace Simulabs_Burse_Console.PriceChanger;

public class RegularPriceChanger(ICollection<IStockMarket.CompanyAndPrice> collection,
    INewPriceCalculator priceCalculator, int sleepTime = 20000) : IPriceChanger
{
    public ICollection<IStockMarket.CompanyAndPrice> Collection { get; } = collection;
    public int SleepTime { get; set; } = sleepTime;
    public INewPriceCalculator PriceCalculator { get; set; } = priceCalculator;
    public Thread PriceChangerThread()
    {
        return new Thread(WorkThread);
    }

    private void WorkThread()
    {
        while (true)
        {
            Thread.Sleep(SleepTime);
            foreach (var companyContainer in Collection)
            {
                companyContainer.Price = PriceCalculator.NewPrice(companyContainer.Price);
            }
        }
    }
}