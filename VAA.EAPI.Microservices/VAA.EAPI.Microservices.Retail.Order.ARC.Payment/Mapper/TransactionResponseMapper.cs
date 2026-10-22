using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Mapper
{
    public class TransactionResponseMapper
    {
        public static Domain.PaymentTransactionResponse Map(Infrastructure.Models.TransactionServiceResponse serviceResponse)
        { 
            if (serviceResponse.Status == "SUCCESS")
            {
                return new Domain.PaymentTransactionResponse
                {
                    RequestId = serviceResponse.RequestId,
                    Status = serviceResponse.Status,
                    TimeStamp = serviceResponse.TimeStamp
                };
            }
            else
            {
                return new Domain.PaymentTransactionResponse
                {
                    errorDetails = serviceResponse.Rejections.Select(r => new ErrorDetails
                    {
                        Message = "TRANSACTION_REJECTED",
                        DeveloperMessage = r.ToString()
                    }),
                    RequestId = serviceResponse.RequestId,
                    Status = serviceResponse.Status,
                    TimeStamp = serviceResponse.TimeStamp
                };
            }
        }
    }
}
