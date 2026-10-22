using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual
{

    /// <summary>
    /// Base Kafka envelope shared by all NLA message types.
    /// </summary>
    public class NlaEnvelopeBase
    {
        [JsonPropertyName("schemaId")]
        public string SchemaId { get; set; } = string.Empty;

        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; }

        [JsonPropertyName("producer")]
        public string Producer { get; set; } = string.Empty;

        [JsonPropertyName("consumer")]
        public string Consumer { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("encryptionKeyId")]
        public string EncryptionKeyId { get; set; } = string.Empty;

        [JsonPropertyName("encryptedPayload")]
        public string EncryptedPayload { get; set; } = string.Empty;

        [JsonPropertyName("producerSignature")]
        public string? ProducerSignature { get; set; }
    }

    public sealed class DbaAccrualEnvelope : NlaEnvelopeBase
    {
        public DbaAccrualEnvelope()
        {
            SchemaId = "nla.accrual.dba";
            SchemaVersion = 1;
        }
    }

    public sealed class RbaAccrualEnvelope : NlaEnvelopeBase
    {
        public RbaAccrualEnvelope()
        {
            SchemaId = "nla.accrual.rba";
            SchemaVersion = 1;
        }
    }

    public sealed class DbaFeedbackEnvelope : NlaEnvelopeBase
    {
        public DbaFeedbackEnvelope()
        {
            SchemaId = Constants.Application.FeedbackDbaSchemaId;
            SchemaVersion = Constants.Application.FeedbackDbaSchemaVersion;
        }
    }

    public sealed class RbaFeedbackEnvelope : NlaEnvelopeBase
    {
        public RbaFeedbackEnvelope()
        {
            SchemaId = "nla.feedback.rba";
            SchemaVersion = 1;
        }
    }

    public sealed class TechnicalErrorEnvelope : NlaEnvelopeBase
    {
        public TechnicalErrorEnvelope()
        {
            SchemaId = "nla.error.technical";
            SchemaVersion = 1;
        }
    }

    /// <summary>
    /// Base fields shared by all decrypted inner payloads.
    /// </summary>
    public class InnerPayloadBase
    {
        [JsonPropertyName("schemaId")]
        public string SchemaId { get; set; } = string.Empty;

        [JsonPropertyName("schemaVersion")]
        public int SchemaVersion { get; set; }
    }

    public sealed class DistanceBasedAccrualPayload : InnerPayloadBase
    {

        [JsonPropertyName("producer")]
        public string Producer { get; set; } = string.Empty;

        [JsonPropertyName("consumer")]
        public string Consumer { get; set; } = string.Empty;

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = string.Empty;

        [JsonPropertyName("accrualType")]
        public string AccrualType { get; set; } = string.Empty;

        [JsonPropertyName("uniqueIdentification")]
        public UniqueIdentification? UniqueIdentification { get; set; }

        [JsonPropertyName("passengerName")]
        public NameDetails? PassengerName { get; set; }

        [JsonPropertyName("ffpDetails")]
        public FfpDetails? FfpDetails { get; set; }

        [JsonPropertyName("flightDetails")]
        public FlightDetails? FlightDetails { get; set; }

        [JsonPropertyName("scheduledDepartureDate")]
        public string ScheduledDepartureDate { get; set; } = string.Empty;

        [JsonPropertyName("flightTypeIndicator")]
        public string? FlightTypeIndicator { get; set; }

        [JsonPropertyName("flightSegment")]
        public FlightSegment? FlightSegment { get; set; }

        [JsonPropertyName("bookingDetails")]
        public BookingClassDetails? BookingDetails { get; set; }

        [JsonPropertyName("documentDetails")]
        public DocumentDetails? DocumentDetails { get; set; }

        [JsonPropertyName("additionalPassengerInformation")]
        public string? AdditionalPassengerInformation { get; set; }

        [JsonPropertyName("creditCode")]
        public string? CreditCode { get; set; }

        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; } = string.Empty;

        [JsonPropertyName("additionalInformation")]
        public string? AdditionalInformation { get; set; }
    }

    public sealed class RevenueBasedAccrualPayload : InnerPayloadBase
    {
        public RevenueBasedAccrualPayload()
        {
            SchemaId = "nla.accrual.rba";
            SchemaVersion = 1;
        }

        [JsonPropertyName("producer")]
        public string Producer { get; set; } = string.Empty;

        [JsonPropertyName("consumer")]
        public string Consumer { get; set; } = string.Empty;

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = string.Empty;

        [JsonPropertyName("accrualType")]
        public string AccrualType { get; set; } = "RBA";

        [JsonPropertyName("uniqueIdentification")]
        public UniqueIdentification UniqueIdentification { get; set; } = new();

        [JsonPropertyName("passengerName")]
        public NameDetails PassengerName { get; set; } = new();

        [JsonPropertyName("ffpDetails")]
        public FfpDetails FfpDetails { get; set; } = new();

        [JsonPropertyName("flightDetails")]
        public FlightDetails FlightDetails { get; set; } = new();

        [JsonPropertyName("scheduledDepartureDate")]
        public string ScheduledDepartureDate { get; set; } = string.Empty;

        [JsonPropertyName("flightTypeIndicator")]
        public string? FlightTypeIndicator { get; set; }

        [JsonPropertyName("flightSegment")]
        public FlightSegment FlightSegment { get; set; } = new();

        [JsonPropertyName("bookingDetails")]
        public BookingClassDetails BookingDetails { get; set; } = new();

        [JsonPropertyName("documentDetails")]
        public DocumentDetails DocumentDetails { get; set; } = new();

        [JsonPropertyName("additionalPassengerInformation")]
        public string? AdditionalPassengerInformation { get; set; }

        [JsonPropertyName("creditCode")]
        public string CreditCode { get; set; } = string.Empty;

        [JsonPropertyName("revenue")]
        public Revenue Revenue { get; set; } = new();

        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; } = string.Empty;

        [JsonPropertyName("additionalInformation")]
        public string? AdditionalInformation { get; set; }
    }

    public sealed class DistanceBasedAccrualFeedbackPayload : InnerPayloadBase
    {
        [JsonPropertyName("producer")]
        public string Producer { get; set; } = string.Empty;

        [JsonPropertyName("consumer")]
        public string Consumer { get; set; } = string.Empty;

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = string.Empty;

        [JsonPropertyName("accrualType")]
        public string AccrualType { get; set; } = "DBA";

        [JsonPropertyName("uniqueIdentification")]
        public UniqueIdentification? UniqueIdentification { get; set; }

        [JsonPropertyName("postingDetails")]
        public PostingDetails? PostingDetails { get; set; }

        [JsonPropertyName("errorInformation")]
        public BusinessErrorInformation? ErrorInformation { get; set; }

        [JsonPropertyName("mileDetails")]
        public MileDetails? MileDetails { get; set; }

        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; } = string.Empty;

        [JsonPropertyName("additionalInformation")]
        public string? AdditionalInformation { get; set; }
    }

    public sealed class RevenueBasedAccrualFeedbackPayload : InnerPayloadBase
    {
        public RevenueBasedAccrualFeedbackPayload()
        {
            SchemaId = "nla.feedback.rba";
            SchemaVersion = 1;
        }

        [JsonPropertyName("producer")]
        public string Producer { get; set; } = string.Empty;

        [JsonPropertyName("consumer")]
        public string Consumer { get; set; } = string.Empty;

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = string.Empty;

        [JsonPropertyName("accrualType")]
        public string AccrualType { get; set; } = "RBA";

        [JsonPropertyName("uniqueIdentification")]
        public UniqueIdentification UniqueIdentification { get; set; } = new();

        [JsonPropertyName("postingDetails")]
        public PostingDetails PostingDetails { get; set; } = new();

        [JsonPropertyName("errorInformation")]
        public BusinessErrorInformation? ErrorInformation { get; set; }

        [JsonPropertyName("mileDetails")]
        public MileDetails MileDetails { get; set; } = new();

        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; } = string.Empty;

        [JsonPropertyName("additionalInformation")]
        public string? AdditionalInformation { get; set; }
    }

    public sealed class TechnicalErrorPayload : InnerPayloadBase
    {
        public TechnicalErrorPayload()
        {
            SchemaId = "nla.error.technical";
            SchemaVersion = 1;
        }

        [JsonPropertyName("producer")]
        public string Producer { get; set; } = string.Empty;

        [JsonPropertyName("consumer")]
        public string Consumer { get; set; } = string.Empty;

        [JsonPropertyName("errorId")]
        public string ErrorId { get; set; } = string.Empty;

        [JsonPropertyName("errorTimestamp")]
        public string ErrorTimestamp { get; set; } = string.Empty;

        [JsonPropertyName("sourceChannel")]
        public string SourceChannel { get; set; } = string.Empty;

        [JsonPropertyName("kafkaPartition")]
        public int KafkaPartition { get; set; }

        [JsonPropertyName("kafkaOffset")]
        public long KafkaOffset { get; set; }

        [JsonPropertyName("kafkaTimestamp")]
        public string? KafkaTimestamp { get; set; }

        [JsonPropertyName("expectedSchemaVersion")]
        public string? ExpectedSchemaVersion { get; set; }

        [JsonPropertyName("receivedSchemaVersion")]
        public string? ReceivedSchemaVersion { get; set; }

        [JsonPropertyName("uniqueIdentification")]
        public UniqueIdentification UniqueIdentification { get; set; } = new();

        [JsonPropertyName("errorInformation")]
        public TechnicalErrorInformation ErrorInformation { get; set; } = new();
    }

    public class UniqueIdentification
    {
        [JsonPropertyName("uniqueIdentificationNumber")]
        public string UniqueIdentificationNumber { get; set; } = string.Empty;

        [JsonPropertyName("airlineCode")]
        public string? AirlineCode { get; set; }
    }

    public class FfpDetails
    {
        [JsonPropertyName("ffpCarrierCode")]
        public string FfpCarrierCode { get; set; } = string.Empty;

        [JsonPropertyName("ffpAccountNumber")]
        public string FfpAccountNumber { get; set; } = string.Empty;
    }

    public class NameDetails
    {
        [JsonPropertyName("surName")]
        public string SurName { get; set; } = string.Empty;

        [JsonPropertyName("givenName")]
        public string GivenName { get; set; } = string.Empty;
    }

    public class BookingClassDetails
    {
        [JsonPropertyName("cabinClassCode")]
        public string CabinClassCode { get; set; } = string.Empty;

        [JsonPropertyName("operatingCarrierBookingClass")]
        public string OperatingCarrierBookingClass { get; set; } = string.Empty;

        [JsonPropertyName("operatingCarrierFareType")]
        public string? OperatingCarrierFareType { get; set; }

        [JsonPropertyName("marketingCarrierBookingClass")]
        public string MarketingCarrierBookingClass { get; set; } = string.Empty;

        [JsonPropertyName("marketingCarrierFareType")]
        public string? MarketingCarrierFareType { get; set; }
    }

    public class FlightDetails
    {
        [JsonPropertyName("marketingFlight")]
        public MarketingFlight MarketingFlight { get; set; } = new();

        [JsonPropertyName("operatingFlight")]
        public OperatingFlight OperatingFlight { get; set; } = new();
    }

    public class MarketingFlight
    {
        [JsonPropertyName("marketingCarrierCode")]
        public string MarketingCarrierCode { get; set; } = string.Empty;

        [JsonPropertyName("marketingFlightNumber")]
        public string MarketingFlightNumber { get; set; } = string.Empty;
    }

    public class OperatingFlight
    {
        [JsonPropertyName("operatingCarrierCode")]
        public string OperatingCarrierCode { get; set; } = string.Empty;

        [JsonPropertyName("operatingFlightNumber")]
        public string? OperatingFlightNumber { get; set; }
    }

    public class FlightSegment
    {
        [JsonPropertyName("origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("destination")]
        public string Destination { get; set; } = string.Empty;
    }

    public class DocumentDetails
    {
        [JsonPropertyName("documentNumber")]
        public string DocumentNumber { get; set; } = string.Empty;

        [JsonPropertyName("documentCouponNumber")]
        public int? DocumentCouponNumber { get; set; }

        [JsonPropertyName("documentIssueDateTime")]
        public string? DocumentIssueDateTime { get; set; }
    }

    public class Revenue
    {
        [JsonPropertyName("revenueDetails")]
        public RevenueDetails RevenueDetails { get; set; } = new();

        [JsonPropertyName("additionalRevenueCreditingDetailsCode")]
        public string? AdditionalRevenueCreditingDetailsCode { get; set; }
    }

    public class RevenueDetails
    {
        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("totalTaxAmount")]
        public decimal TotalTaxAmount { get; set; }

        [JsonPropertyName("billingAmount")]
        public decimal BillingAmount { get; set; }

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;
    }

    public class BusinessErrorInformation
    {
        [JsonPropertyName("errorDetails")]
        public List<BusinessErrorDetail> ErrorDetails { get; set; } = [];
    }

    public class BusinessErrorDetail
    {
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;

        [JsonPropertyName("errorDescription")]
        public string? ErrorDescription { get; set; }
    }

    public class TechnicalErrorInformation
    {
        [JsonPropertyName("errorDetails")]
        public List<TechnicalErrorDetail> ErrorDetails { get; set; } = [];
    }

    public class TechnicalErrorDetail
    {
        [JsonPropertyName("errorCode")]
        public string ErrorCode { get; set; } = string.Empty;

        [JsonPropertyName("errorDescription")]
        public string ErrorDescription { get; set; } = string.Empty;

        [JsonPropertyName("field")]
        public string Field { get; set; } = string.Empty;

        [JsonPropertyName("providedValue")]
        public string? ProvidedValue { get; set; }
    }

    public class PostingDetails
    {
        [JsonPropertyName("postingDateTime")]
        public string PostingDateTime { get; set; } = string.Empty;

        [JsonPropertyName("postingStatus")]
        public string PostingStatus { get; set; } = string.Empty;
    }

    public class MileDetails
    {
        [JsonPropertyName("baseMiles")]
        public int? BaseMiles { get; set; }

        [JsonPropertyName("classOfServiceBonusMiles")]
        public int? ClassOfServiceBonusMiles { get; set; }

        [JsonPropertyName("eliteTierBonusMiles")]
        public int? EliteTierBonusMiles { get; set; }

        [JsonPropertyName("promotionalBonusMiles")]
        public int? PromotionalBonusMiles { get; set; }
    }

    /// <summary>
    /// Kafka header defined by the AsyncAPI message trait.
    /// </summary>
    public class KafkaHeaders
    {
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;
    }
}
