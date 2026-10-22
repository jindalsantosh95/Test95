using System;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Reshop
{
    public class ReshopResponseToDocumentMapper
    {
        public static CosmosDocumentEvent DocumentMapper(OfferResponse reshopResponse, string docId, string docType)
        {
            return new CosmosDocumentEvent
            {
                Document = new OfferDocument
                {
                    Id = docId,
                    PartitionKey = docId,
                    CreatedBy = Constants.Application.NdcChannel,
                    CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                    DocType = docType,
                    OrderId = reshopResponse.OrderId,
                    ShoppingResponseId = reshopResponse.ShoppingResponseId,
                    FlightOfferResponse = new()
                    {
                        Pos = reshopResponse.Pos,
                        Parties = reshopResponse.Parties,
                        DataList = reshopResponse.DataList,
                        Offers = reshopResponse.Offers
                    }
                },
                ModificationType = CosmosModificationType.Create
            };
        }
    }
}
