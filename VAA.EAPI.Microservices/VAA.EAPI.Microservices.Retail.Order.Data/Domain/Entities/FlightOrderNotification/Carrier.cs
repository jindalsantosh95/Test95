using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class Carrier
    {
        public string FlightNum { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierName { get; set; }

        public static explicit operator Carrier(SegmentDetail segmentDetail)
        {
            return new Carrier
            {
                FlightNum = segmentDetail?.FlightNumber,
                CarrierCode = segmentDetail?.OperatingCarrier,
                CarrierName = segmentDetail?.OperatingCarrier
            };
        }
    }
}
