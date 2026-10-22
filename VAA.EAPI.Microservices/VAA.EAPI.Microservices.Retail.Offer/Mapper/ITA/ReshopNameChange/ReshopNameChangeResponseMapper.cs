using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.ReshopNameChange
{
    public class ReshopNameChangeResponseMapper
    {
        public static async Task<OfferResponse> ReshopResponseMapper(ReshopRequest reshopRequest, string docId, string offerExpirationLimit)
        {
            var reshopResponse = new OfferResponse
            {
                Parties = reshopRequest.Parties,
                OrderId = reshopRequest.OrderId
            };

            reshopResponse.Offers = await GetReshopOffers(reshopRequest, docId, offerExpirationLimit);
            return reshopResponse;
        }

        private static async Task<IEnumerable<OfferDetails>> GetReshopOffers(ReshopRequest reshopRequest, string docId, string offerExpirationLimit)
        {
            return new[]
              {
                 new OfferDetails
                   {
                            OfferId = $"{docId}",
                            OfferItems = await GetOfferItems(reshopRequest, docId),
                            OfferExpirationTimeLimit = offerExpirationLimit
                    }
               };
        }

        private static async Task<IEnumerable<OfferItem>> GetOfferItems(ReshopRequest reshopRequest, string docId)
        {
            int offerItemId = 1;

              return new[]
               {
                  new OfferItem
                     {
                        OfferItemId = $"{docId}-{offerItemId}",
                        PaxNameChangeDetails = new()
                             {
                               GivenName = reshopRequest?.PaxNameChangeDetails?.GivenName,
                               Surname = reshopRequest?.PaxNameChangeDetails?.Surname,
                               PaxRefID = reshopRequest?.PaxNameChangeDetails?.PaxRefID
                             }
                      }
               };
        }

    }
}