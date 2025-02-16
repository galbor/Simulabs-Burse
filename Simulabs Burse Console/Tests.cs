using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simulabs_Burse_Console.Trader;

namespace Simulabs_Burse_Console
{
    //if ids are the same for two tests, might cause issues
    internal static class Tests
    {
        /**
         * tests:
         * buy stock    V
         * sell stock   V
         * make sell offer for unavailable stock    X but I tested it by hand by accident
         * make buy offer for price too high    V
         * buy stock for cheaper than cheapest offer  V
         * buy stock for offered price and not prev offer price     V
         * sell stock for offered price and not prev offer price    V
         * sell stock for more than biggest buy offer   V
         * stock price change after buy     V
         * stock price change after many seconds    X but I saw it happen
         * buy the cheapest stock   V
         * sell to the highest bidder   V
         * make buy offer, then lose all money, then find seller    V
         * check trade history  X but observed to be working ok
         * check portfolios     X but observed to be working ok
         * remove conflicting offers    V
         * multiple sales at once   V
         * multithreading   V
         */

        public static void RunAllTests()
        {
            Thread t1 = new Thread(DoAllTestsThread);
            t1.Start();
            t1.Join();
        }
        private static void DoAllTestsThread()
        {
            string richtraderId = "1";
            string richtraderId2 = "2";
            string poortraderId = "3";
            string poortraderId2 = "4";
            string cheapcompanyId = "14";

            TestBuy(poortraderId);

            EmptyStock(richtraderId, cheapcompanyId);

            TestSell(richtraderId, poortraderId2, cheapcompanyId);
            TestAbsurdSale(richtraderId, poortraderId);
            TestMultithreading(richtraderId, richtraderId2);
        }

        /**
         * tests:
         * buying stock from previous offer
         * buying stock at previous offer's price
         * buying stock at cheaper than offered price (should fail)
         *
         */
        public static bool TestBuy(string traderId)
        {
            string stockid = "17";
            string pricystockid = "16";
            bool res = true;

            ITrader trader = StockMarket.GetTraderFromId(traderId);
            if (trader == null)
            {
                Console.Error.WriteLine("wrong trader id for TestBuy(): " + traderId);
                return false;
            }

            decimal prevMoney = trader.Money;

            ICompany company = StockMarket.GetCompanyFromId(stockid); //100,000,000 shares for 1$
            ICompany pricyCompany = StockMarket.GetCompanyFromId(pricystockid);

            StockMarket.MakeOffer(trader, company, prevMoney + 1, 1, false);
            StockMarket.MakeOffer(trader, pricyCompany, 1, 1, false);
            while (StockMarket.GetTraderOffers(traderId).Length == 0)
            {
                Thread.Sleep(2); //wait for the offer to pass
                if (trader.HasStock(pricystockid))
                {
                    Console.Error.WriteLine("TestBuy() failed - bought stock for price too low");
                    res = false;
                    break;
                }
            }
            if (trader.HasStock(stockid))
            {
                Console.Error.WriteLine("TestBuy() failed - bought too expensive stock");
                res = false;
            }
            prevMoney = trader.Money;
            uint stockAmt = trader.StockAmount(stockid);
            decimal salePrice = 5;
            decimal companyPrice = company.Price;
            StockMarket.MakeOffer(trader, company, salePrice, 1,false);
            while (trader.StockAmount(stockid) == stockAmt)
                Thread.Sleep(1);

            if (trader.Money == prevMoney - salePrice)
            {
                Console.Error.WriteLine("TestBuy() failed - bought stock for written price and not prev offer price");
                return false;
            }

            if (trader.Money == prevMoney - companyPrice)
            {
                Console.WriteLine("TestBuy() success - bought stock for prev offer price");
                return res;
            }

            StringBuilder errorMessage = new StringBuilder();
            errorMessage.Append("Test Buy Failed() - bought stock for different money amount:\n");
            errorMessage.AppendFormat("Money now should be {0} - {1} = {2}\tand not {3}", prevMoney, companyPrice, prevMoney-companyPrice, trader.Money);
            Console.Error.WriteLine(errorMessage.ToString());
            return false;
        }


        /**
         * tests:
         * sell stock for offered price and not prev offer price
         * sell stock for more than biggest buy offer
         * stock price change after buy
         * buy cheapest stock
         * sell to highest bidder
         * remove conflicting offers
         * multiple sales at once
         */
        private static void TestSell(string richtraderid, string poortraderid, string companyid)
        {
            ITrader richTrader = StockMarket.GetTraderFromId(richtraderid);
            ITrader poorTrader = StockMarket.GetTraderFromId(poortraderid);
            ICompany company = StockMarket.GetCompanyFromId(companyid);

            List<IOffer> offers = new List<IOffer>();

            decimal prevMoney = richTrader.Money;

            offers.Add(StockMarket.MakeOffer(richTrader, company, 10, 2, true));
            offers.Add(StockMarket.MakeOffer(richTrader, company, 1, 3, true));
            offers.Add(StockMarket.MakeOffer(richTrader, company, 8, 1, true));

            offers.Add(StockMarket.MakeOffer(poorTrader, company, 11, 5, false));
            while (richTrader.Money == prevMoney)
            {
                Thread.Sleep(1);
            }

            if (richTrader.Money != prevMoney + 21)
            {
                StringBuilder errMessage = new StringBuilder("TestSell() failed - sale price was wrong.\n");
                errMessage.AppendFormat("Rich trader's money should be {0} + 1*3 + 8*1 + 10*1 = {1} and not {2}",
                    prevMoney, prevMoney + 21, richTrader.Money);
                Console.Error.WriteLine(errMessage.ToString());
            }

            if (StockMarket.GetTraderOffers(poortraderid).Length > 0)
            {
                Console.Error.WriteLine(
                    "TestSell() fail - sell to multiple buy offers (finished) didn't update correctly");
            }

            if (company.Price != 10)
            {
                Console.Error.WriteLine("company price should be 10 now instead of " + company.Price);
            }

            StockMarket.RemoveOffers(offers);
            offers.Clear();

            while (StockMarket.HasPendingRequests())
            {
                Thread.Sleep(1);
            }

            prevMoney = richTrader.Money;

            offers.Add(StockMarket.MakeOffer(poorTrader, company, 11, 2, false));
            offers.Add(StockMarket.MakeOffer(poorTrader, company, 12, 2, false));
            offers.Add(StockMarket.MakeOffer(poorTrader, company, 10, 2, false));

            offers.Add(StockMarket.MakeOffer(richTrader, company, 11, 5, true));

            while (richTrader.Money == prevMoney)
                Thread.Sleep(1);

            if (richTrader.Money != prevMoney + 46)
            {
                StringBuilder errMessage = new StringBuilder("TestSell() failed - sale price was wrong.\n");
                errMessage.AppendFormat("Rich trader's money should be {0} + 11*2 +12*2 = {1} and not {2}",
                    prevMoney, prevMoney + 46, richTrader.Money);
                Console.Error.WriteLine(errMessage.ToString());
            }

            {
                var offer = StockMarket.GetTraderOffers(richtraderid)[0];
                if (offer.Price != 11 || offer.Amount != 1)
                {
                    Console.Error.WriteLine(
                        "TestSell() fail - sell to multiple buy offers (unfinished) didn't update correctly");
                }
            }

            prevMoney = poorTrader.Money;

            offers.Add(StockMarket.MakeOffer(richTrader, company, 1, 1, true));
            offers.Add(StockMarket.MakeOffer(richTrader, company, 1, 1, true));
            offers.Add(StockMarket.MakeOffer(richTrader, company, 1, 1, false));

            while (StockMarket.GetTraderOffers(richtraderid).Length == 0)
            {
                Thread.Sleep(1);
            }

            if (StockMarket.GetTraderOffers(richtraderid).Length != 1)
            {
                Console.Error.WriteLine("TestSell() fail - didn't delete conflicting error");
            }
            else
            {
                Console.WriteLine("TestSale() success!");
            }
        }

        /**
         * make buy offer, then lose all money, then find seller
         * volatile for some reason
         */
        private static void TestAbsurdSale(string stockownerid, string poortraderid)
        {
            ITrader stockOwner = StockMarket.GetTraderFromId(stockownerid);
            ITrader poorTrader = StockMarket.GetTraderFromId(poortraderid);
            var idamtpair = StockMarket.GetTraderInfo(stockownerid).Portfolio[0];
            ICompany company = StockMarket.GetCompanyFromId(idamtpair.Key);

            RemoveAllCompanyOffers(idamtpair.Key);

            uint prevAmt = stockOwner.StockAmount(idamtpair.Key);
            decimal prevMoney = stockOwner.Money;

            StockMarket.MakeOffer(poorTrader, company, poorTrader.Money * 0.75M, 1, false);
            StockMarket.MakeOffer(poorTrader, company, poorTrader.Money * 0.75M, 1, false);
            StockMarket.MakeOffer(stockOwner, company, 1, 2, true);

            while (stockOwner.Money == prevMoney)
            {
                Thread.Sleep(1);
            }

            if (stockOwner.StockAmount(idamtpair.Key) != prevAmt - 1)
                Console.Error.WriteLine("TestAbsurdSale() fail - Wrong amount of stock now");
            else
                Console.WriteLine("TestAbsurdSale() success!");
        }

        /**
         * tests multithreading
         * does lots of sales
         * stockowner should have lots of stocks and trader should have lots of money
         */
        private static void TestMultithreading(string stockownerid, string traderid)
        {
            ITrader stockOwner = StockMarket.GetTraderFromId(stockownerid);
            ITrader trader = StockMarket.GetTraderFromId(traderid);
            var idamtpair = StockMarket.GetTraderInfo(stockownerid).Portfolio[0];
            ICompany company = StockMarket.GetCompanyFromId(idamtpair.Key);

            RemoveAllCompanyOffers(idamtpair.Key);

            uint prevAmt = stockOwner.StockAmount(idamtpair.Key);
            decimal prevMoney = stockOwner.Money;
            uint iterations = idamtpair.Value / 16;
            uint amtSold = iterations * 8;


            void sellAction()
            {
                for (int i = 0; i < iterations; i++)
                {
                    StockMarket.MakeOffer(stockOwner, company, 3, 2, true);
                    StockMarket.MakeOffer(stockOwner, company, 3, 1, true);
                    StockMarket.MakeOffer(stockOwner, company, 3, 1, true);

                    Thread.Sleep(1);
                }
            }

            void buyAction ()
            {
                for (int i = 0; i < iterations; i++)
                {
                    StockMarket.MakeOffer(trader, company, 3, 2, false);
                    StockMarket.MakeOffer(trader, company, 3, 1, false);
                    StockMarket.MakeOffer(trader, company, 3, 1, false);
                    Thread.Sleep(1);
                }
            }

            var tsell1 = new Thread(sellAction);
            var tsell2 = new Thread(sellAction);
            var tbuy1 = new Thread(buyAction);
            var tbuy2 = new Thread(buyAction);

            tsell1.Start();
            tsell2.Start();
            tbuy1.Start();
            tbuy2.Start();

            tsell1.Join();
            tsell2.Join();
            tbuy1.Join();
            tbuy2.Join();

            uint cnt = 0;
            while (StockMarket.HasPendingRequests())
            {
                Thread.Sleep(1);
                cnt++;
            }

            Console.WriteLine("Waited " + cnt + " milliseconds for TestMultithreading() to finish processing offers");

            if (stockOwner.Money != prevMoney + amtSold * 3)
            {
                
                Console.Error.WriteLine("TestMultithreading() failed - wrong amount of money.\n");
            }

            if (stockOwner.StockAmount(idamtpair.Key) != prevAmt - amtSold)
            {
                
                Console.Error.WriteLine("TestMultithreading() failed - wrong amount of stocks sold.\n");
            }
            StringBuilder message = new StringBuilder();
            message.AppendFormat("stockOwner's money should be {0} + {1} * 3 = {2} and it is {3}\n",
                prevMoney, amtSold, prevMoney + amtSold * 3, stockOwner.Money);
            message.AppendFormat("stockOwner's stocks should be {0} - {1} = {2} and it is {3}",
                prevAmt, amtSold, prevAmt - amtSold, stockOwner.StockAmount(idamtpair.Key));
            Console.WriteLine(message.ToString());

            Console.WriteLine("TestMultithreading() done");
        }

        /**
         * makes trader buy all stock of company
         */
        private static void EmptyStock(string traderId, string companyId)
        {
            ITrader trader = StockMarket.GetTraderFromId(traderId);
            ICompany company = StockMarket.GetCompanyFromId(companyId);

            IOffer[] offers = StockMarket.GetCompanyOffers(companyId);
            foreach (IOffer offer in offers)
            {
                if (offer.IsSellOffer)
                    StockMarket.MakeOffer(trader, company, offer.Price, offer.Amount, false);
            }
            while (trader.StockAmount(companyId) < 10)
                Thread.Sleep(1);
        }

        private static void RemoveAllCompanyOffers(string companyid)
        {
            while (StockMarket.GetCompanyOffers(companyid).Length != 0)
            {
                StockMarket.RemoveOffers(StockMarket.GetCompanyOffers(companyid));
                Thread.Sleep(1);
            }
            while (StockMarket.HasPendingRequests())
            {
                Thread.Sleep(1);
            }
            
        }
    }
}
