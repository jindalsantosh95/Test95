namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class SeatOffers
    {
        public Offer Offer { get; set; }
        public SeatOffer.SeatAvailability SeatAvailability { get; set; }
        public string OfferWarnings { get; set; }

    }
}
