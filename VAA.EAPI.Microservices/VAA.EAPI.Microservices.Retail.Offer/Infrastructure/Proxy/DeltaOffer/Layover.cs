namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Layover
    {
        public string ArrivalFlightNum { get; set; }
        public Carrier ArrivalMarketingCarrier { get; set; }
        public Carrier ArrivalOperatingCarrier { get; set; }
        public bool ChangeOfAirport { get; set; }
        public string DepartureFlightNum { get; set; }
        public Carrier DepartureMarketingCarrier { get; set; }
        public Carrier DepartureOperatingCarrier { get; set; }
        public string DestinationAirportCode { get; set; }
        public bool EquipmentChange { get; set; }
        public string LayoverAirportCode { get; set; }
        public Duration LayoverDuration { get; set; }
        public string OriginAirportCode { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
    }
}
