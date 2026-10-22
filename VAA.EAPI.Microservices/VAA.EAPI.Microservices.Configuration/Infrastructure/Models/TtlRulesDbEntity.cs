using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class TtlRulesDbEntity : TableEntity
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
