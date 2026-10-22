using System;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class NameChangeOfferDocumentMapper
    {
        public static OfferDocument RedisDocumentMapper(string docId, string docType, ReshopRequest requestData, OfferResponse reshopResponse)
        {
            return new OfferDocument
            {
                Id = docId,
                PartitionKey = docId,
                CreatedBy = Constants.Application.NdcChannel,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                DocType = docType,
                PaxNameChangeDetails = requestData?.PaxNameChangeDetails,
                OrderId = requestData?.OrderId,
                FlightOfferResponse = new()
                {
                    DataList = reshopResponse.DataList,
                    Offers = reshopResponse.Offers
                }
            };
        }
    }
}
