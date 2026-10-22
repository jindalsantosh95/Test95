using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;

namespace VAA.Facade.NDC.IFGFacade.Mapper
{
    public class PaymentResponseMapper
    {
        public static PaymentResponse MapPaymentResponse(PaymentMSResponse paymentMSResponse, bool isAuth)
        {
            var response = paymentMSResponse.PaymentDocument != null ? new PaymentResponse
            {
                HttpStatus = 200,
                PaymentStatus = "AUTH_ACCEPTED",
                CaptureStatus = isAuth ? "PENDING" : "COMPLETED",
                TransactionId = paymentMSResponse.PaymentDocument.TransactionId
            }
            : new PaymentResponse
            {
                Errors = paymentMSResponse.Errors
            };

            return response;
        }
    }
}
