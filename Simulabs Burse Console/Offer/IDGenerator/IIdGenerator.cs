namespace Simulabs_Burse_Console.Offer.IDGenerator;

public interface IIdGenerator<T>
{
    /**
     * @return next unique ID
     */
    public T Next();
}