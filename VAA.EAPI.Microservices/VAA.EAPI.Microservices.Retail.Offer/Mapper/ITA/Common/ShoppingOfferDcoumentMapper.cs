using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.Common
{
    public class ShoppingOfferDcoumentMapper
    {
        public static OfferResponse ShoppingOfferDcoument(OfferResponse offerResponse)
        {
            return new OfferResponse
            {
                Pos = offerResponse.Pos,
                Parties = offerResponse.Parties,
                FareTypeRequest = new()
                {
                    FareTypeCode = offerResponse?.FareTypeRequest?.FareTypeCode,
                    FareSubTypeCode = offerResponse?.FareTypeRequest?.FareSubTypeCode
                },
                Offers = offerResponse.Offers.Select(offer => new OfferDetails
                {
                    OfferId = offer.OfferId,
                    OfferExpirationTimeLimit = offer.OfferExpirationTimeLimit,
                    ItaAccountCode = offer.ItaAccountCode,
                    TicketingTimeLimit = offer.TicketingTimeLimit,
                    OfferItems = GetOfferItem(offer.OfferItems),
                    SalePrice = offer.SalePrice.Where(x => x.PriceComponentType == Price.PriceComponentTypes.BasePriceEnum),
                }),
                ShoppingResponseId = offerResponse.ShoppingResponseId,
                DataList = new DataList
                {
                    CustomerDetails = offerResponse.DataList.CustomerDetails,
                    PriceClass = offerResponse.DataList.PriceClass
                }
            };
        }

        private static IEnumerable<OfferItem> GetOfferItem(IEnumerable<OfferItem> offerItems)
        {
            return offerItems.Select(offerItem => new OfferItem
            {
                OfferItemId = offerItem.OfferItemId,
                Services = GetServices(offerItem.Services),
                ServiceBeneficiaries = offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries,
                AirlineTaxonomies = offerItem.AirlineTaxonomies,
                TicketDesigCode = offerItem.TicketDesigCode
            });
        }

        private static IEnumerable<OfferService> GetServices(IEnumerable<OfferService> services)
        {
            return services.Select(service => new OfferService
            {
                PriceClassRefIds = service.PriceClassRefIds,
                ServiceBeneficiaries = service.ServiceBeneficiaries
            });
        }
    }
}
