namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
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
        public Link Link { get; set; }
        public bool? AppliedToAllPassengers { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public string Status { get; set; }
        public MessageSeverityCodeType MessageSeverity { get; set; }

        public static explicit operator MessageDetail(Infrastructure.Proxy.DeltaMessageCentre.MessageDetail source)
        {
            if (source == null)
                return null;

            return new MessageDetail
            {
                Ordinal = source.Ordinal,
                MessageCode = source.MessageCode,
                CodeType = (MessageCodeType)source.CodeType,
                Type = source.Type,
                SubType = source.SubType,
                MessageContextTypeCode = (MessageContextCodeType)source.MessageContextTypeCode,
                TripId = source.TripId,
                SegmentId = source.SegmentId,
                HeaderDesc = source.HeaderDesc,
                BodyDesc = source.BodyDesc,
                AppliedToAllPassengers = source.AppliedToAllPassengers,
                Status = source.Status,
                MessageSeverity = (MessageSeverityCodeType)source.MessageSeverity,
                Id = source.Id,
                Link = (Link)source.Link,
                Passengers = source.Passengers?.Select(p => (Passenger)p)
            };
        }
    }
}
