using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("Upsell_UpsellOfferResponse")]
    public class UpsellOfferResponse
    {
        public string BookingId { get; set; }
        public bool UpsellEligible { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<Segment> Segments { get; set; }

        public static explicit operator UpsellOfferResponse((Infrastructure.Models.UpsellOffer.UpsellOfferResponse, string tripReservationId, IEnumerable<AirportRefData> airportRefdata, IEnumerable<CabinRefData> cabinRefData, IEnumerable<AirlineRefData> airlineRefData) source)
        {
            var (response, tripReservationId, airportRefdata, cabinRefData, airlineRefData) = source;
            if (response == null) return null;

            return new UpsellOfferResponse
            {
                BookingId = tripReservationId,
                UpsellEligible = response.UpsellEligible,
                Slices = response.Slices?.Select(s => (Slice)(s, airportRefdata)),
                Segments = response.Segments?.Select(s => (Segment)(s, airportRefdata, cabinRefData, airlineRefData))
            };
        }
    }
}
