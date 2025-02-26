using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console.Factories;

public interface IStockMarketFactory
{
    public ICompany NewCompany(string id, string name, decimal price);
    public ITrader NewTrader(string id, string name, decimal money);
    /**
     * trader should start with @param amount of @pram company 's stock in portfolio
     */
    public ITrader NewCompanyTrader(ICompany company, uint amount);
    public IOffer NewOffer(ICompany company, ITrader trader, decimal price, uint amount, bool isSellOffer);
}