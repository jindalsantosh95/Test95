using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual
{
    public sealed class AutoAccrualInboundEvent : CloudEvent
    {
        public new AutoAccrualInboundData? Data
        {
            get => base.Data as AutoAccrualInboundData;
            set => base.Data = value;
        }
    }

    public sealed class AutoAccrualInboundData
    {
        public string? CorrelationId { get; init; }
        public string? MessageId { get; init; }
        public AutoAccrualRequest? AccrualRequest { get; init; }
    }

    public sealed class AutoAccrualRequest
    {
        public string? SchemaId { get; init; }
        public int? SchemaVersion { get; init; }
        public string? Producer { get; init; }
        public string? Consumer { get; init; }
        public string? RequestType { get; init; }
        public string? AccrualType { get; init; }
        public UniqueIdentification? UniqueIdentification { get; init; }
        public PassengerName? PassengerName { get; init; }
        public FfpDetails? FfpDetails { get; init; }
        public FlightDetails? FlightDetails { get; init; }
        public string? ScheduledDepartureDate { get; init; }
        public string? FlightTypeIndicator { get; init; }
        public FlightSegment? FlightSegment { get; init; }
        public BookingDetails? BookingDetails { get; init; }
        public DocumentDetails? DocumentDetails { get; init; }
        public string? ProductCode { get; init; }
        public string? AdditionalInformation { get; init; }
    }

    public sealed class UniqueIdentification { public string? UniqueIdentificationNumber { get; init; } public string? AirlineCode { get; init; } }
    public sealed class PassengerName { public string? SurName { get; init; } public string? GivenName { get; init; } }
    public sealed class FfpDetails { public string? FfpCarrierCode { get; init; } public string? FfpAccountNumber { get; init; } }
    public sealed class FlightDetails { public Flight? MarketingFlight { get; init; } public Flight? OperatingFlight { get; init; } }
    public sealed class Flight { public string? MarketingCarrierCode { get; init; } public string? MarketingFlightNumber { get; init; } public string? OperatingCarrierCode { get; init; } public string? OperatingFlightNumber { get; init; } }
    public sealed class FlightSegment { public string? Origin { get; init; } public string? Destination { get; init; } }
    public sealed class BookingDetails { public string? CabinClassCode { get; init; } public string? OperatingCarrierBookingClass { get; init; } public string? MarketingCarrierBookingClass { get; init; } }
    public sealed class DocumentDetails { public string? DocumentNumber { get; init; } public int? DocumentCouponNumber { get; init; } public string? DocumentIssueDateTime { get; init; } }
}
