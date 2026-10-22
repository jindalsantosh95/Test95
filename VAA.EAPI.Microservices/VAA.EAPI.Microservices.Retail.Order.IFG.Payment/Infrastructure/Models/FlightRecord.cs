namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class FlightRecord
    {
        public bool ConjunctionTicketIndicator { get; set; }
        public PassengerRecord PassengerRecord { get; set; }
        public DocumentDetails DocumentDetails { get; set; }
        public DocumentID DocumentID { get; set; }
        public string DateOfIssuance { get; set; }
        public string AutomaticRepricingEngine { get; set; }
        public string StatisticalCode { get; set; }
    }
}
