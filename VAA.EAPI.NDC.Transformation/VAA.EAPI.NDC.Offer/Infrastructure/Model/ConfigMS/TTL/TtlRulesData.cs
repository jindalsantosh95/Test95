namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.ConfigMS.TTL
{
    public class TtlRulesData
    {
        public int FareTypeCode { get; set; }
        public string FareSubTypeCode { get; set; }
        public string OperatingCarrier { get; set; }
        public int? MinDepartureDate { get; set; }
        public int? MaxDepartureDate { get; set; }
        public int? TTLAfterBooking { get; set; }
        public int? TTLBeforeDeparture { get; set; }
    }
}
