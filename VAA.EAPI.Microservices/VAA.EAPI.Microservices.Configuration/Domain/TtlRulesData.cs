using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Domain
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

        public static explicit operator TtlRulesData(TtlRulesDbEntity entity)
        {
            return new TtlRulesData()
            {
                FareTypeCode = entity.FareTypeCode,
                FareSubTypeCode = entity.FareSubTypeCode,
                OperatingCarrier = entity.OperatingCarrier,
                MinDepartureDate = entity.MinDepartureDate,
                MaxDepartureDate = entity.MaxDepartureDate,
                TTLAfterBooking = entity.TTLAfterBooking,
                TTLBeforeDeparture = entity.TTLBeforeDeparture
            };
        }
    }
}
