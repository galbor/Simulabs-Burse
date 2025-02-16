using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console.POD
{
    public readonly struct Sale(string sellerId, string buyerId, string companyId, decimal price, uint amount)
    {
        public readonly string SellerId = sellerId;
        public readonly string BuyerId = buyerId;
        public readonly string CompanyId = companyId;
        public readonly decimal Price = price; //per stock
        public readonly uint Amount = amount;
        public readonly DateTime Time = DateTime.Now;

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.AppendFormat("Seller id: {0}\nBuyer id: {1}\nCompany id: {2}\nPrice per stock:{3}\nAmount: {4}\n",
                              SellerId, BuyerId, CompanyId, Price, Amount);
            res.AppendFormat("Time: {0}\n", Time);
            return res.ToString();
        }
    }
}
