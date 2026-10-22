using VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.Amenity
{
    public class CabinAmenity
    {
        public CabinDetails CabinDetails { get; set; }
        public List<string>? Errors { get; set; }
        public int Legs => CabinDetails?.Legs.Count() ?? 0;
        [GraphQLDeprecated("Links will always be empty")]
        public IEnumerable<Link>? Links { get; set; }

        public CabinAmenity() { }
        public CabinAmenity(Brand.Brand brandResponse, IEnumerable<OfferDetails> offers, IEnumerable<FlightOffer.Segment> offerResponseSegments,
            bool isAwardSearchResult, IEnumerable<AirlineInfo> airlineInfo, IEnumerable<CabinInfo> cabinInfo)
        {
            CabinDetails = new CabinDetails(brandResponse, offers, offerResponseSegments, isAwardSearchResult, airlineInfo, cabinInfo);
            Errors = brandResponse.Errors;
        }
    }
}