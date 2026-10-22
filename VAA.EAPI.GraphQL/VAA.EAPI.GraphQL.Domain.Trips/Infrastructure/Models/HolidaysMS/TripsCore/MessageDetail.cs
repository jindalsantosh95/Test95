using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class MessageDetail
    {
        public int? Ordinal { get; set; }
        public string MessageCode { get; set; }
        public MessageCodeType CodeType { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public string Id { get; set; }
        public MessageContextCodeType MessageContextTypeCode { get; set; }
        public string TripId { get; set; }
        public string SegmentId { get; set; }
        public string HeaderDesc { get; set; }
        public string BodyDesc { get; set; }
        public bool? AppliedToAllPassengers { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public string Status { get; set; }
        public MessageSeverityCodeType MessageSeverity { get; set; }

    }
}
