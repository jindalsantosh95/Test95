
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.UpsellOffer
{
    public class UpsellOfferResponse
    {
        public string BookingId { get; set; }
        public bool UpsellEligible { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
    }
}
