
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation
{
    public class SegmentDtl
    {
        public string AirlineCode { get; set; }
        public string ArrivalAirport { get; set; }
        public string ClassOfService { get; set; }
        public string ClassOfService2 { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureAirport { get; set; }
        public string FlightNumber { get; set; }
        public string CurrentActionCode { get; set; }
    }
}
