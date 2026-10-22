namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class UpsellOfferRequest
    {
        public BookingReference BookingReference { get; set; }
        public RequestOptionCodeType Using { get; set; }
        public string ConfirmationNum { get; set; }
        public string EncryptedConfirmationNum { get; set; }
        public string TicketNum { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
    }
}
