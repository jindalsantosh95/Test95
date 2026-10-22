using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.CPD
{
    public class PaymentAccountHelper
    {
        public static string GetPaymentAccount(bool isUsAgent, bool isMoto)
        {
            if (isUsAgent)
            {
                return isMoto
                    ? ConfigReader.Get(Constants.Environment.ARCMotoAccount)
                    : ConfigReader.Get(Constants.Environment.ARCEcomAccount);
            }

            return isMoto
                ? ConfigReader.Get(Constants.Environment.CPDMotoAccount)
                : ConfigReader.Get(Constants.Environment.CPDAccount);
        }
    }
}
