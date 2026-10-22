
namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class UpsellOfferResponse
    {
        public string BookingId { get; set; }
        public bool UpsellEligible { get; set; }
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<Segment> Segments { get; set; }

        public static explicit operator UpsellOfferResponse(Infrastructure.Proxy.DeltaUpsellOffers.UpsellOfferResponse source)
        {
            if(source == null)
                return null;    

            return new UpsellOfferResponse
            {
                BookingId = source.ReservationResourceId,
                UpsellEligible = source.UpsellEligible,
                Slices = source.Trips?.Select(slice => (Slice)slice),
                Segments = source.Segments?.Select(segment => (Segment)segment)
            };  
        }
    }
}
