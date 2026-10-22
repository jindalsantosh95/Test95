using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.VoucherRedemption
{
    public sealed class VoucherFlightDetails
    {
        public string ActivityDate { get; init; }
        public string BookingClass { get; init; }
        public string OriginAirport { get; init; }
        public string DestinationAirport { get; init; }
        public string BookingReference { get; init; }
        public string OperatingFlightNumber { get; init; }
        [JsonProperty("noOfSegments")]
        public int NumberOfSegments { get; init; }
        [JsonProperty("noOfPassenger")]
        public int NumberOfPassengers { get; init; }
        public string MarketingFlightNumber { get; init; }
        public string OperatingCarrierCode { get; init; }
        public string MarketingCarrierCode { get; init; }
    }
}
