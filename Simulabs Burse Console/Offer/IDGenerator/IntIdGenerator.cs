namespace Simulabs_Burse_Console.Offer.IDGenerator;

public class IntIdGenerator : IIdGenerator<int>
{
    private object _lock = new object();
    private int _id = 0;
    public IntIdGenerator() { }
    public int Next()
    {
        lock (_lock)
        {
            return _id++;
        }
    }
}