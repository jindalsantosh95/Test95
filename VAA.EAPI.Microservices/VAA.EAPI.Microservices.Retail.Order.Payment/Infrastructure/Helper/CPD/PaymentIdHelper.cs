using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.CPD
{
    public class PaymentIdHelper
    {
        public static string GeneratePaymentId()
        {
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Random rand = new Random();

            int dRandom = rand.Next(00000000, 99999999);
            string strFinalNum = Convert.ToString(unixTimestamp) + Convert.ToString(dRandom);
            
            return strFinalNum;
        }
    }
}
