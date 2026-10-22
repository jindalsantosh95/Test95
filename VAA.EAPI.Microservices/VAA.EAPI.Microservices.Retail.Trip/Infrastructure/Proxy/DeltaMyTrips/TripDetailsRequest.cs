namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class TripDetailsRequest
    {
        public RequestOptionCodeType Using { get; set; }
        public string ConfirmationNum { get; set; }
        public string EncryptedConfirmationNum { get; set; }
        public string TicketNum { get; set; }
        public string CreditCardNum { get; set; }
        public string LoyaltyNum { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string DepartureAirportCode { get; set; }

    }
}
