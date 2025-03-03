using System.Threading;

namespace Simulabs_Burse_Console.PriceChanger;

public interface IPriceChanger
{
    public Thread PriceChangerThread();
}