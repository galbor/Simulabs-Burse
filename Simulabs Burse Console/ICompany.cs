using Simulabs_Burse_Console.POD;

namespace Simulabs_Burse_Console;

public interface ICompany
{
    public string Id { get; }
    public string Name {get;}

    /**
     * returns recent sales
     * with arr[0] being the oldest and arr[^1] being the newest
     */
    public Sale[] GetRecentSales();

    /**
     * adds sale to history
     * throws exception if sale is of wrong company
     */
    public void AddSale(Sale sale);
}