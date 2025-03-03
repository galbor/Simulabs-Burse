using System.Collections.Generic;
using Simulabs_Burse_Console.Company;
using Simulabs_Burse_Console.Offer;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console.Stock_Market;

public interface IStockMarket
{
    public static abstract IStockMarket Instance { get; }

    /**
    * Starts working
    * must be initiated
    * @return true iff wasn't initiated before
    */
    public bool Init();

    /**
    * throws exception if company doesn't exist
    */
    public CompanyInfo GetCompanyInfo(string id);

    /**
     * throws exception if trader doesn't exist
     */
    public TraderInfo GetTraderInfo(string id);

    /**
     * returns null if no such trader exists
     */
    public ITrader GetTraderFromId(string id);

    /**
     * returns null if no such company exists
     */
    public ICompany GetCompanyFromId(string id);

    public void CreateTrader(Dictionary<string, object> jsonDictionary);
    public void CreateTrader(ITrader trader);
    public void CreateCompany(Dictionary<string, object> jsonDictionary);

    /**
     * @return true iff previously made a request and the stock market didn't process it yet
     */
    public bool HasPendingRequests();

    /**
     * makes request to make offer
     * @return fitting offer
     */
    public IOffer MakeOffer(ITrader trader, ICompany company, decimal price, uint amount, bool isSellOffer);
    /**
     * make request to remove offer(s)
     */
    public void RemoveOffer(IOffer offer);
    public void RemoveOffers(IEnumerable<IOffer> offers);

    /**
     * @return different array so the real list can't be edited
     */
    public IOffer[] GetTraderOffers(string id);
    /**
     * @return different array so the real list can't be edited
     */
    public IOffer[] GetCompanyOffers(string id);

    public List<ITrader> GetAllTraders();
    public List<ICompany> GetAllCompanies();

    protected class CompanyAndPrice(ICompany company, decimal price)
    {
        public ICompany Company { get; } = company;
        public decimal Price { get; set; } = price;
    }
}