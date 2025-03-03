using System;
using Simulabs_Burse_Console.Utility;

namespace Simulabs_Burse_Console.PriceChanger;

public class GaussianNewPriceCalculator(decimal divider = 10M, decimal distanceFromZero = 0.5M) : INewPriceCalculator
{
    public decimal Divider { get; set; } = divider;
    public decimal DistanceFromZero { get; set; } = distanceFromZero;

    public decimal NewPrice(decimal prevPrice)
    {
            decimal stdDev = prevPrice / Divider;
            decimal distanceFromZero = DistanceFromZero;
            return Math.Abs(MyUtils.NormalDistribution(prevPrice, stdDev) - distanceFromZero) + distanceFromZero;
    }
}