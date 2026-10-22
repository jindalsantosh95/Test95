using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper.CPD
{
    public class GetTransactionStatusMapper
    {
        public static GetTransactionStatus GetTransactionStatusRequest(string paymentTransactionId)
        {
            return new GetTransactionStatus
            {
                ClientId = ConfigReader.Get(Constants.Environment.CPDClientId),
                Transactions = new Transactions
                {
                    transactionId = paymentTransactionId,
                }
            };
        }
    }
}
