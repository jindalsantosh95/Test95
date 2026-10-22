using System;
using System.Linq;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reprice
{
    public class RepriceResponseToDocumentMapper
    {
        public static OfferDocument CreateDocumentMapper(OfferResponse repriceResponse, string offerId, string docType)
        {
            return new OfferDocument
            {
                Id = offerId,
                PartitionKey = offerId,
                CreatedBy = Constants.Application.NdcChannel,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                DocType = docType,
                FareTypeRequest = new()
                {
                    FareTypeCode = repriceResponse?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = repriceResponse?.FareTypeRequest?.FareSubTypeCode
                },
                ShoppingResponseId = repriceResponse.ShoppingResponseId,
                OrderId = repriceResponse.OrderId,
                FlightOfferResponse = new()
                {
                    DataList = repriceResponse.DataList,
                    Pos = repriceResponse.Pos,
                    Parties = repriceResponse.Parties,
                    Offers = repriceResponse.Offers.Where(x => x.OfferId == offerId).ToList(),
                }
            };
        }
    }
}
