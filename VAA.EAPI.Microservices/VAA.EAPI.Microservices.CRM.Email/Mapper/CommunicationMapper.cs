using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.CRM.Email.Domain;

namespace VAA.EAPI.Microservices.CRM.Email.Mapper
{
    public class CommunicationMapper
    {
        public static SendEmailRequest RefundFailureRequestMapper(RefundFailureRequest request)
        {
            return new SendEmailRequest
            {
                From = ConfigReader.Get(Constants.Environment.RefundFailureFromAddress),
                To = ConfigReader.Get(Constants.Environment.RefundFailureToAddress),
                Cc = ConfigReader.Get(Constants.Environment.RefundFailureCcAddress),
                Subject = string.Format(ConfigReader.Get(Constants.Environment.RefundFailureSubject), request.TransactionId),
                Template = ConfigReader.Get(Constants.Environment.RefundFailureTemplate),
                DataFields = request
            };
        }
    }
}
