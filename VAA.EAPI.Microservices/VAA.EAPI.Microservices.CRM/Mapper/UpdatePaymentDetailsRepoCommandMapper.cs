using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System.Net.Http.Headers;
using VAA.EAPI.Microservices.CRM.Domain;
using VAA.EAPI.Microservices.CRM.Domain.CRMSchema;
using VAA.EAPI.Microservices.CRM.Infrastructure.Repository;

namespace VAA.EAPI.Microservices.CRM.Mapper
{
    public class UpdatePaymentDetailsRepoCommandMapper
    {
        public static UpdatePaymentDetailsRepoCommand MaptoUpdateReqeustCommand(PaymentRequest paymentRequest, string paymentId, string countryId, string customerCountryid)
        {
            return new UpdatePaymentDetailsRepoCommand()
            {
                paymentEntity = GetPaymentEntity(paymentRequest, paymentId, countryId, customerCountryid)
            };
        }

        private static PaymentEntity GetPaymentEntity(PaymentRequest paymentRequest, string paymentId, string countryId, string customerCountryid)
        {
            var request = new PaymentEntity()
            {
                VaaPaymentid = paymentId,
                VaaCountryCode = $"/{Constants.Application.CRMPaymentEntity}({countryId})",
                VaaBeneficiaryName = paymentRequest.AccountHolderName,
                VaaAccountnumber = paymentRequest.AccountNumber,
                VaaClabenumber = paymentRequest.ClabeNumber,
                VaaBankcode = paymentRequest.SortCode,
                VaaIfsc = paymentRequest.IfscCode,
                VaaBankname = paymentRequest.BankName,
                VaaIban = paymentRequest.Iban,
                VaaBeneficiaryaddress = paymentRequest.PayeeAddress,
                VaaSwift = paymentRequest.SwiftCode,
                VaaRoutingcode = paymentRequest.RoutingCode,
                VaaBankaccountdetailsentered = true,
                VaaBankcodenumber = paymentRequest.BankCodeNumber,
                VaaBanksuffixnumber = paymentRequest.AccountSuffix,
                VaaStreetname = paymentRequest.Street,
                VaaTowncityname = paymentRequest.Town,
                VaaStateprovincecounty = paymentRequest.State,
                VaaPostcode = paymentRequest.PostCode
            };


            if (!customerCountryid.IsNullOrWhiteSpace())
                request.VaaCustomerCountry = $"/{Constants.Application.CRMPaymentEntity}({paymentRequest.CustomerCountryId})";

            return request;

        }

        public static PaymentResponse MaptoUpdatePaymentResponse(HttpResponseHeaders paymentUpdateResponse, string correlationId)
        {
            return new PaymentResponse()
            {
                Status = paymentUpdateResponse.Location is not null ? Domain.Enums.StatusEnum.Success : Domain.Enums.StatusEnum.Failure,
                CorrelationId = correlationId
            };
        }
    }
}
