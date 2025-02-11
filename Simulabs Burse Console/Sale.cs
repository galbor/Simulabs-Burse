using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulabs_Burse_Console
{
    internal struct Sale
    {
        public readonly string SellerId;
        public readonly string BuyerId;
        public readonly string CompanyId;
        public readonly decimal Price; //per stock
        public readonly uint Amount;
        public readonly DateTime Time;

        public Sale(string sellerId, string buyerId, string companyId, decimal price, uint amount)
        {
            this.SellerId = sellerId;
            this.BuyerId = buyerId;
            this.CompanyId= companyId;
            this.Price = price;
            this.Amount = amount;

            Time = DateTime.Now;
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            res.AppendFormat("Seller id: {0}\nBuyer id: {1}\nCompany id: {2}\nPrice per stock:{3}\nAmount: {4}\n",
                              SellerId,       BuyerId,       CompanyId,       Price,               Amount);
            res.AppendFormat("Time: {0}\n", Time);
            return res.ToString();
        }
    }
}
