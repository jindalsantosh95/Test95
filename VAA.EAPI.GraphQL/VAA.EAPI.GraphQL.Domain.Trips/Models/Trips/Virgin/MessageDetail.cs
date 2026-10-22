using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin
{
    public class MessageDetail
    {
        public int? Ordinal { get; set; }
        public string? MessageCode { get; set; }
        public MessageCodeType? CodeType { get; set; }
        public string? Type { get; set; }
        public string? SubType { get; set; }
        public string? Id { get; set; }
        public MessageContextCodeType? MessageContextTypeCode { get; set; }
        public string? SliceId { get; set; }
        public string? SegmentId { get; set; }
        public string?   HeaderDesc { get; set; }
        public string? BodyDesc { get; set; }
        public bool? AppliedToAllPassengers { get; set; }
        public IEnumerable<Passenger>? Passengers { get; set; }
        public string? Status { get; set; }
        public MessageSeverityCodeType? MessageSeverity { get; set; }

        public static explicit operator MessageDetail(Infrastructure.Models.TripMS.MessageDetail source)
        {
            if (source == null) return null;

            return new MessageDetail
            {
                Ordinal = source.Ordinal,
                MessageCode = source.MessageCode,
                CodeType = (MessageCodeType)source.CodeType,
                Type = source.Type,
                SubType = source.SubType,
                Id = source.Id,
                MessageContextTypeCode = (MessageContextCodeType)source.MessageContextTypeCode,
                SliceId = source.TripId,
                SegmentId = source.SegmentId,
                HeaderDesc = source.HeaderDesc,
                BodyDesc = source.BodyDesc,
                AppliedToAllPassengers = source.AppliedToAllPassengers,
                Status = source.Status,
                MessageSeverity = (MessageSeverityCodeType)source.MessageSeverity,
                Passengers = source.Passengers?.Select(p => (Passenger)(p, Enumerable.Empty<CabinRefData>())),
            };
        }
    }
}