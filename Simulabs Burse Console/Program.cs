﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Simulabs_Burse_Console.Company;
using Simulabs_Burse_Console.Offer;
using Simulabs_Burse_Console.POD;
using Simulabs_Burse_Console.Trader;
using Simulabs_Burse_Console.Utility;
using Simulabs_Burse_Console.Stock_Market;

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
        private static string allstocks = "all stocks info";

        private static IStockMarket _stockMarket = StockMarket.Instance;

        static void Main(string[] args)
        {
            string input;

            Console.WriteLine("Please enter json path:");
            while (true)
            {
                //input = GetInput();
                input = "BurseJson.json";
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

            _stockMarket.Init();
            Tests.RunAllTests(_stockMarket);

            while (true)
            {
                Console.WriteLine("Please write command. write \"help\" for help.");
                input = GetInput();
                if (input == quit) return;
                if (input == help)
                {
                    PrintHelpStatement(help,"for help");
                    PrintHelpStatement(quit,"to quit");
                    PrintHelpStatement(getnames,"to get all traders' names");
                    PrintHelpStatement(gettraderinfo,"and the trader id to get said trader's info");
                    PrintHelpStatement(getcompanyinfo,"and the company id to get said company's stock info");
                    PrintHelpStatement(gettraderhistory,"and the trader id to get the trader's recent sale history");
                    PrintHelpStatement(makesaleoffer,"to make a sale offer");
                    PrintHelpStatement(makebuyoffer,"to make a buy offer");
                    PrintHelpStatement(deleteoffer,"to delete an offer");
                    PrintHelpStatement(allstocks, "to learn about all the stocks");
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

                if (input == allstocks)
                {
                    foreach (var info in AllStocksInfo())
                    {
                        Console.WriteLine(info.ToString() + "\n");
                    }
                }
            }
        }

        private static void PrintHelpStatement(string command, string description)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Write \'{0}\' {1}", command, description);
            Console.WriteLine(sb.ToString());
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
                _stockMarket.CreateCompany(share);
            }

            foreach (var trader in traders)
            {
                _stockMarket.CreateTrader(trader);
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
                TraderInfo info = GetTraderInfo(id);
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
                CompanyInfo info = GetCompanyInfo(id);
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
                Sale[] history = _stockMarket.GetTraderFromId(id).GetRecentSales();
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
            ITrader trader;
            int offerId;

            Console.WriteLine("Please write the trader's ID:");
            string input = GetInput();
            while ((trader = _stockMarket.GetTraderFromId(input)) == null)
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

            RemoveOffer(trader.Id, offerId);
        }

        private static void MakeOfferInConsole(Func<ITrader, ICompany, decimal, uint, IOffer> makeOfferFunc)
        {
            ITrader trader;
            ICompany company;
            decimal price;
            uint amount;

            Console.WriteLine("Please write the trader's ID:");
            string input = GetInput();
            while ((trader = _stockMarket.GetTraderFromId(input)) == null)
            {
                if (input == quit) return;
                Console.WriteLine("wrong ID, please try again. To abort write " + quit);
                input = GetInput();
            }

            Console.WriteLine("Please write the company's ID:");
            input = GetInput();
            while ((company = _stockMarket.GetCompanyFromId(input)) == null)
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

        static List<CompanyInfo> AllStocksInfo()
        {
            return _stockMarket.GetAllCompanies().Select(company => _stockMarket.GetCompanyInfo(company.Id)).ToList();
        }

        static List<string[]> AllTraderNames()
        {
            return _stockMarket.GetAllTraders().Select(trader => new string[]{trader.Id, trader.Name }).ToList();
        }

        static CompanyInfo GetCompanyInfo(string id)
        {
            return _stockMarket.GetCompanyInfo(id);
        }

        static TraderInfo GetTraderInfo(string id)
        {
            return _stockMarket.GetTraderInfo(id);
        }

        static Sale[] GetRecentTraderHistory(string id)
        {
            return _stockMarket.GetTraderFromId(id).GetRecentSales();
        }

        static IOffer MakeSaleOffer(ITrader trader, ICompany company, decimal price, uint amount)
        {
            return _stockMarket.MakeOffer(trader, company, price, amount, true);
        }

        static IOffer MakeBuyOffer(ITrader trader, ICompany company, decimal price, uint amount)
        {
            return _stockMarket.MakeOffer(trader, company, price, amount, false);
        }

        static void RemoveOffer(string traderId, int offerId)
        {
            var offerlst = GetTraderInfo(traderId).Offers.Where(offer => offer.OfferId == offerId).ToList();
            if (offerlst.Count == 0) return;
            IOffer offer = offerlst[0];
            _stockMarket.RemoveOffer(offer);
        }
    }
}
