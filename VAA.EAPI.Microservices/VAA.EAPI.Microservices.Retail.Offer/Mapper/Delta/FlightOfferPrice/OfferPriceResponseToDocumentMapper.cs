using System;
using System.Linq;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlightOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Delta.FlightOfferPrice
{
    public class OfferPriceResponseToDocumentMapper
    {
        public static CosmosDocumentEvent CreateDocumentMapper(string offerId, FlightOfferResponse offerpriceRs)
        {
            return new CosmosDocumentEvent()
            {
                Document = new OfferDocument
                {
                    Id = offerId,
                    PartitionKey = offerId,
                    CreatedBy = Constants.Application.SearchOfferChannelID,
                    CreatedOn = DateTime.UtcNow.ToString(Constants.Application.IsoDateTimeFormat),
                    DocType = Constants.Application.OfferPriceDocType,
                    FlightOfferResponse = new()
                    {
                        Pos = offerpriceRs.Pos,
                        Parties = offerpriceRs.Parties,
                        DataList = offerpriceRs.DataList,
                        Offers = offerpriceRs.Offers.Where(x => x.OfferId == offerId)
                    }
                },
                ModificationType = CosmosModificationType.Create
            };
        }
    }
}
