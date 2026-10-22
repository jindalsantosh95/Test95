namespace VAA.Facade.NDC.IFGFacade.Domain
{
    public class FlightRecord
    {
        public DocumentDetails DocumentDetails { get; set; }
        public bool ConjunctionTicketIndicator { get; set; }
        public PassengerRecord PassengerRecord { get; set; }
        public DocumentID DocumentID { get; set; }
        public string DateOfIssuance { get; set; }
        public string AutomaticRepricingEngine { get; set; }
        public string StatisticalCode { get; set; }
    }

}
