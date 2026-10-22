namespace VAA.EAPI.Microservices.Retail.Trip.Domain.UpsellDetails
{
    public class UpsellOffer
    {
        public string OfferItemNumber { get; set; }
        public CabinClass CabinClass { get; set; }
        public IEnumerable<UpsellOfferDetail> UpsellOfferDetails { get; set; }
        public int OfferPriority { get; set; }
        public int DisplayPriority { get; set; }
        public int AvailableSeatsRemaining { get; set; }

        public static explicit operator UpsellOffer(Infrastructure.Proxy.DeltaUpsellOffers.UpsellOffer source)
        {
            if (source == null) return null;
            return new UpsellOffer
            {
                OfferItemNumber = source.OfferItemNumber,
                CabinClass = (CabinClass)source.CabinClass,
                UpsellOfferDetails = source.UpsellOfferDetails?.Select(detail => (UpsellOfferDetail)detail),
                OfferPriority = source.OfferPriority,
                DisplayPriority = source.DisplayPriority,
                AvailableSeatsRemaining = source.AvailableSeatsRemaining
            };
        }
    }

}