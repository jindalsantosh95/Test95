namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class PassengerDocumentSummary
    {
        public Price AvailableCredit { get; set; }
        public Price ChangeFee { get; set; }
        public Price CancelFee { get; set; }
        public string PassengerRefId { get; set; }
        public Price TicketValue { get; set; }
    }
}
