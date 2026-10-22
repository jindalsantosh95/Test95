namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class SeatLocation
    {
        public string SeatStatus { get; set; }
        public string SeatColumnName { get; set; }
        public int SeatRowNum { get; set; }
        public string SeatProductCode { get; set; }
        public bool EconomyComfort { get; set; }
        public bool ExitRow { get; set; }
        public bool ExitOverride { get; set; }
        public string FlightLegId { get; set; }
    }
}