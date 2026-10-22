using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer.SummaryDeepLink;
using VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Mapper
{
    public class OfferMSSummaryDeepLinkRequestMapper
    {
        public static FlightDeepLinkSummaryRequest Map(SearchSummaryDeepLinkQueryRequest request, AirportInfo? posDetails)
        {
            var offerRequest = new FlightDeepLinkSummaryRequest()
            {
                MetaPartnerId = request.MetaPartnerId,
                CustomerDetails = GetCustomerDetails(request.Customers),
                Pos = new()
                {
                    CityCode = posDetails?.PosCityCode,
                    CountryCode = posDetails?.CountryCode
                },
                TotalOfferedAmount = new()
                {
                    Currency = request.TotalOfferedAmount.Currency,
                    Value = request.TotalOfferedAmount.Value.ToString()
                },
                LoyaltyMember = new() { PayWithPoints = true },
                DominantSegmentBrandId = request.DominantSegmentBrandId,
                CurrentTripIndexId = (request.FlightSegments.Max(x => x.SliceNumber) + 1).ToString(),
                FlightSegments = request.FlightSegments.Select((fs, index) => (DeepLinkSegment)(fs, index))
            };
            return offerRequest;
        }

        private static IEnumerable<Customer> GetCustomerDetails(PaxBreakdown customers)
        {
            var customerDetails = new List<Customer>();
            int customerIndex = 1;

            // Add Adults
            for (int i = 0; i < customers.NoOfAdults; i++)
            {
                customerDetails.Add(new Customer
                {
                    CustId = $"ADULT_{customerIndex}",
                    Ptc = PtcEnums.PtcTypeEnum.ADT.ToString()
                });
                customerIndex++;
            }

            // Add Young Adults
            for (int i = 0; i < customers.NoOfYoungAdults; i++)
            {
                customerDetails.Add(new Customer
                {
                    CustId = $"TEEN_{customerIndex}",
                    Ptc = PtcEnums.PtcTypeEnum.GBE.ToString()
                });
                customerIndex++;
            }

            // Add Children
            for (int i = 0; i < customers.NoOfChildren; i++)
            {
                customerDetails.Add(new Customer
                {
                    CustId = $"CHILD_{customerIndex}",
                    Ptc = PtcEnums.PtcTypeEnum.CNN.ToString()
                });
                customerIndex++;
            }

            // Add Infants
            for (int i = 0; i < customers.NoOfInfants; i++)
            {
                customerDetails.Add(new Customer
                {
                    CustId = $"INFANT_{customerIndex}",
                    Ptc = PtcEnums.PtcTypeEnum.INF.ToString()
                });
                customerIndex++;
            }

            return customerDetails;
        }
    }
}
