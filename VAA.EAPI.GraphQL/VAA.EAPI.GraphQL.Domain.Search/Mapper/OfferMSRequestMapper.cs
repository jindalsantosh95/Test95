using VAA.EAPI.GraphQL.Domain.Search.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS;

namespace VAA.EAPI.GraphQL.Domain.Search.Mapper
{
    public class OfferMSRequestMapper
    {
        public static FlightOfferRequest BasketToOfferMSRequestMapper(Basket basketMsresponse, string offerId, string tripIndex)
        {
            return new()
            {
                FlightSearchRequest = new()
                {
                    SearchOriginDestinations = basketMsresponse.FlightsearchRequest.SearchOriginDestinations.Select(ond => (Models.FlightOffer.SearchOriginDestination)ond),
                    CurrentTripIndexId = tripIndex,
                    OfferID = offerId,
                    BrandId = basketMsresponse.FlightsearchRequest?.BrandId,
                    AwardSearch = basketMsresponse.FlightsearchRequest?.isAwardSearch ?? false,
                    PromoCode = basketMsresponse.FlightsearchRequest?.PromoCode
                },
                LoyaltyMember = new()
                {
                    PayWithPoints = basketMsresponse.FlightsearchRequest.isAwardSearch ? false : true
                },
                CustomerDetails = MapCustomers(basketMsresponse.People)
            };
        }

        public static FlightOfferPriceRequest BasketToOfferMSPriceRequestMapper(Basket basketMsresponse, string offerId)
        {
            return new()
            {
                FlightSearchRequest = new()
                {
                    SearchOriginDestinations = basketMsresponse.FlightsearchRequest.SearchOriginDestinations.Select(ond => (Models.FlightOffer.SearchOriginDestination)ond),
                    CurrentTripIndexId = basketMsresponse.FlightsearchRequest.SearchOriginDestinations.Count().ToString(),
                    OfferID = offerId,
                    BrandId = basketMsresponse.FlightsearchRequest?.BrandId,
                    AwardSearch = basketMsresponse.FlightsearchRequest?.isAwardSearch ?? false,
                    PromoCode = basketMsresponse.FlightsearchRequest?.PromoCode
                },
                LoyaltyMember = new()
                {
                    PayWithPoints = basketMsresponse.FlightsearchRequest.isAwardSearch ? false : true
                },
                CustomerDetails = MapCustomers(basketMsresponse.People)
            };
        }

        private static IEnumerable<Customer> MapCustomers(IEnumerable<People> basketPeople)
        {
            int adt = 1, gbe = 1, cnn = 1, inf = 1;
            IEnumerable<Customer> customers = Enumerable.Empty<Customer>();

            foreach (People customer in basketPeople)
            {

                switch (customer.Type)
                {
                    case nameof(PtcEnums.PtcTypeEnum.ADT):
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.ADT}",
                            CustId = $"{PtcEnums.PtcTypeEnum.ADT}_{adt++}"
                        });
                        break;

                    case nameof(PtcEnums.PtcTypeEnum.GBE):
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.GBE}",
                            CustId = $"{PtcEnums.PtcTypeEnum.GBE}_{gbe++}"
                        });
                        break;

                    case nameof(PtcEnums.PtcTypeEnum.CNN):
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.CNN}",
                            CustId = $"{PtcEnums.PtcTypeEnum.CNN}_{cnn++}"
                        });
                        break;

                    case nameof(PtcEnums.PtcTypeEnum.INF):
                        customers = customers.Append(new()
                        {
                            Ptc = $"{PtcEnums.PtcTypeEnum.INF}",
                            CustId = $"{PtcEnums.PtcTypeEnum.INF}_{inf++}"
                        });
                        break;
                }
            }
            return customers;
        }
    }
}
