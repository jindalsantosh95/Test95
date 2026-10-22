using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Insurance.Domain.Decision;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzDecisionRequest;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Mappers
{
    public static class AllianzDecisionRequestMapper
    {
        public static AllianzDecisionRequest AllianzDecisionRequest(DecisionRequest domainRequest)
        {
            return new AllianzDecisionRequest
            {
                Credentials = new Credentials { Username = ConfigReader.Try(Constants.Environment.AllianzQuoteCreateUsername), Password = ConfigReader.Try(Constants.Environment.AllianzQuoteCreatePassword) },
                BookingId = domainRequest.BookingId,
                BusinessUnit = domainRequest.BusinessUnit,
                CurrencyType = domainRequest.CurrencyType,
                Cxid = domainRequest.Cxid,
                ResponseId = domainRequest.ResponseId,
                ResponseType = domainRequest.ResponseType,
                Details = domainRequest.Details.Select(d => new Proxy.AllianzDecisionRequest.Detail
                {
                    ResponseType = d.ResponseType,
                    SupplierCode = d.SupplierCode,
                    ProductCode = d.ProductCode,
                    PurchasePrice = d.PurchasePrice,
                    Quantity = d.Quantity,
                    ResponseQuoteId = d.ResponseQuoteId
                }).ToArray()
            };
        }
    }
}
