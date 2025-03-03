namespace Simulabs_Burse_Console;

public interface IIdGenerator<T>
{
    /**
     * @return next unique ID
     */
    public T Next();
}