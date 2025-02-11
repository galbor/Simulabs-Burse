using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console
{
    internal class Program
    {
        private static string quit = "q";
        private static string help = "help";
        private static string getnames = "getnames";
        private static string gettraderinfo = "TInfo";
        private static string getcompanyinfo = "CInfo";
        private static string gettraderhistory = "THistory";
        private static string makesaleoffer = "sell";
        private static string makebuyoffer = "buy";
        private static string deleteoffer = "delete";



        static void Main(string[] args)
        {
            string input;

            Console.WriteLine("Please enter json path:");
            while (true)
            {
                input = GetInput();
                //input = "BurseJson.json";
                try
                {
                    var json = LoadJson(input);
                    InitStockMarket(LoadJson(input));
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Loading Json failed with an exception, please try again.\n" + e.Message);
                }
            }

            StockMarket.StartThreads();
            //Tests.RunAllTests();

            while (true)
            {
                Console.WriteLine("Please write command. write \"help\" for help.");
                input = GetInput();
                if (input == quit) return;
                if (input == help)
                {
                    Console.WriteLine("Write " + help + " for help");
                    Console.WriteLine("Write " + quit + " to quit");
                    Console.WriteLine("Write " + getnames + " to get all traders' names");
                    Console.WriteLine("Write " + gettraderinfo + " and the trader id to get said trader's info");
                    Console.WriteLine("Write " + getcompanyinfo + " and the company id to get said company's stock info");
                    Console.WriteLine("Write " + gettraderhistory + " and the trader id to get the trader's recent sale history");
                    Console.WriteLine("Write " + makesaleoffer + " to make a sale offer");
                    Console.WriteLine("Write " + makebuyoffer + " to make a buy offer");
                    Console.WriteLine("Write " + deleteoffer + " to delete an offer");
                    continue;
                }

                if (input == getnames)
                {
                    PrintAllTraderNames();
                    continue;
                }

                if (input.StartsWith(gettraderinfo))
                {
                    string id = MyUtils.StringAfterCommand(input, gettraderinfo);
                    PrintTraderInfo(id);
                    continue;
                }

                if (input.StartsWith(getcompanyinfo))
                {
                    string id = MyUtils.StringAfterCommand(input, getcompanyinfo);
                    PrintCompanyInfo(id);
                    continue;
                }

                if (input.StartsWith(gettraderhistory))
                {
                    string id = MyUtils.StringAfterCommand(input, gettraderhistory);
                    PrintTraderHistory(id);
                    continue;
                }

                if (input == makesaleoffer)
                {
                    MakeOfferInConsole(MakeSaleOffer);
                    continue;
                }
                if (input == makebuyoffer)
                {
                    MakeOfferInConsole(MakeBuyOffer);
                    continue;
                }

                if (input == deleteoffer)
                {
                    DeleteOfferInConsole();
                }
            }
        }

        private static string GetInput()
        {
            Console.Write(">");
            return Console.ReadLine();
        }

        /**
         * @return Dictionary if successful
         */
        private static Dictionary<string, Dictionary<string, object>[]> LoadJson(string path)
        {
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>[]>>(File.ReadAllText(path));
        }

        private static void InitStockMarket(Dictionary<string, Dictionary<string, object>[]> json)
        {
            Dictionary<string, object>[] shares = (Dictionary<string, object>[])json["shares"];
            Dictionary<string, object>[] traders = (Dictionary<string, object>[])json["traders"];

            foreach (var share in shares)
            {
                StockMarket.CreateCompany(share);
            }

            foreach (var trader in traders)
            {
                StockMarket.CreateTrader(trader);
            }
        }

        private static void PrintAllTraderNames()
        {
            foreach (var idname in AllTraderNames())
            {
                Console.WriteLine(idname[0] + ". " + idname[1]);
            }
        }

        private static void PrintTraderInfo(string id)
        {
            try
            {
                Trader.TraderInfo info = GetTraderInfo(id);
                Console.WriteLine(info.ToString());
            }
            catch
            {
                Console.WriteLine("wrong trader id");
            }
        }

        private static void PrintCompanyInfo(string id)
        {
            try
            {
                Company.CompanyInfo info = GetCompanyInfo(id);
                Console.WriteLine(info.ToString());
            }
            catch
            {
                Console.WriteLine("wrong company id");
            }
        }

        private static void PrintTraderHistory(string id)
        {
            try
            {
                Sale[] history = StockMarket.GetTraderFromId(id).GetRecentSales();
                Console.WriteLine("Recent sale history:\n");
                foreach (var sale in history)
                {
                    Console.WriteLine("\t" + sale.ToString());
                }
            }
            catch
            {
                Console.WriteLine("wrong trader id");
            }
        }

        private static void DeleteOfferInConsole()
        {
            Trader trader;
            int offerId;

            Console.WriteLine("Please write the trader's ID:");
            string input = GetInput();
            while ((trader = StockMarket.GetTraderFromId(input)) == null)
            {
                if (input == quit) return;
                Console.WriteLine("wrong ID, please try again. To abort write " + quit);
                input = GetInput();
            }

            Console.WriteLine("Please write the offer Id (integer)");
            input = GetInput();
            while (!int.TryParse(input, out offerId))
            {
                if (input == quit) return;
                Console.WriteLine("Please write a number for the price. To abort write " + quit);
                input = GetInput();
            }

            RemoveOffer(trader._id, offerId);
        }

        private static void MakeOfferInConsole(Func<Trader, Company, decimal, uint, Offer> makeOfferFunc)
        {
            Trader trader;
            Company company;
            decimal price;
            uint amount;

            Console.WriteLine("Please write the trader's ID:");
            string input = GetInput();
            while ((trader = StockMarket.GetTraderFromId(input)) == null)
            {
                if (input == quit) return;
                Console.WriteLine("wrong ID, please try again. To abort write " + quit);
                input = GetInput();
            }

            Console.WriteLine("Please write the company's ID:");
            input = GetInput();
            while ((company = StockMarket.GetCompanyFromId(input)) == null)
            {
                if (input == quit) return;
                Console.WriteLine("wrong ID, please try again. To abort write " + quit);
                input = GetInput();
            }

            Console.WriteLine("Please write the price per stock (positive number)");
            input = GetInput();
            while ((price = MyUtils.StringToDecimal(input)) <= 0)
            {
                if (input == quit) return;
                Console.WriteLine("Please write a positive number for the price. To abort write " + quit);
                input = GetInput();
            }

            Console.WriteLine("Please write the amount (positive number)");
            input = GetInput();
            while ((amount = MyUtils.StringToUInt(input)) <= 0)
            {
                if (input == quit) return;
                Console.WriteLine("Please write a positive number for the price. To abort write " + quit);
                input = GetInput();
            }

            makeOfferFunc(trader, company, price, amount);
        }

        static List<Company.CompanyInfo> AllStocksInfo()
        {
            return StockMarket.GetAllCompanies().Select(company => new Company.CompanyInfo(company)).ToList();
        }

        static List<string[]> AllTraderNames()
        {
            return StockMarket.GetAllTraders().Select(trader => new string[]{trader._id, trader._name }).ToList();
        }

        static Company.CompanyInfo GetCompanyInfo(string id)
        {
            return new Company.CompanyInfo(StockMarket.GetCompanyFromId(id));
        }

        static Trader.TraderInfo GetTraderInfo(string id)
        {
            return new Trader.TraderInfo(StockMarket.GetTraderFromId(id));
        }

        static Sale[] GetRecentTraderHistory(string id)
        {
            return StockMarket.GetTraderFromId(id).GetRecentSales();
        }

        static Offer MakeSaleOffer(Trader trader, Company company, decimal price, uint amount)
        {
            return StockMarket.MakeOffer(trader, company, price, amount, true);
        }

        static Offer MakeBuyOffer(Trader trader, Company company, decimal price, uint amount)
        {
            return StockMarket.MakeOffer(trader, company, price, amount, false);
        }

        static void RemoveOffer(string traderId, int offerId)
        {
            var offerlst = GetTraderInfo(traderId).Offers.Where(offer => offer.OfferId == offerId).ToList();
            if (offerlst.Count == 0) return;
            Offer offer = offerlst[0];
            StockMarket.RemoveOffer(offer);
        }
    }
}
