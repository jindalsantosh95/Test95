namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class TravelRelatedServiceTax
    {
        public string CalculationMethodText { get; set; }
        public string ChargeTypeCode { get; set; }
        public string TaxCode { get; set; }
        public string TaxSubCode { get; set; }
        public Price TaxValue { get; set; }
        public Price CostDifferenceAmt { get; set; }
        public string DestinationAirportCode { get; set; }
        public string OriginAirportCode { get; set; }
        public Price PreviousTaxValue { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
        public string TaxStatusCode { get; set; }
        public string TaxDesc { get; set; }
        public ValueAddTax ValueAddTax { get; set; }
    }
}
