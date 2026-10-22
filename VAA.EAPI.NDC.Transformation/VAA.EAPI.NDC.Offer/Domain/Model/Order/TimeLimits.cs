namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class TimeLimits
    {
        public LocalAndUTCDateTime OfferTL { get; set; }
        public LocalAndUTCDateTime PaymentTL { get; set; }
        public LocalAndUTCDateTime OrderGuaranteeTL { get; set; }
        public LocalAndUTCDateTime TicketTL { get; set; }
        public LocalAndUTCDateTime OfferPriceGuaranteeTL { get; set; }
        public LocalAndUTCDateTime NamingTL { get; set; }

    }
}
