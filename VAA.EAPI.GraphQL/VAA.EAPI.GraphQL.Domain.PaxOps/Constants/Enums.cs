using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Constants
{
    public enum BookingType
    {
        UNKNOWN, // Default / null value
        AIRLINE,
        HOTEL
    }

    public enum OverallCheckInStatus
    {
        UNKNOWN, // Default / null value
        TO_COMPLETE,
        PART_COMPLETE,
        COMPLETE
    }
    public enum ContactTracingStatus
    {
        UNKNOWN, // Default / null value
        NOT_REQUIRED,
        TO_COMPLETE,
        COMPLETE,
        NEEDED_LATER
    }
    public enum ReturnDateStatus
    {
        UNKNOWN, // Default / null value
        PROVIDED,
        REQUIRED,
        NOT_REQUIRED,
        PROVIDE_PASSPORT_FIRST
    }

    public enum SecondaryDocumentStatus
    {
        UNKNOWN, // Default / null value
        REQUIRED,
        COMPLETE,
        NOT_REQUIRED,
        PROVIDE_PASSPORT_FIRST
    }

    public enum GovernmentFormsStatus
    {
        REQUIRED,
        COMPLETED
    }

    public enum TemplateCode
    {
        UNKNOWN,
        VS_ECONOMY,
        VS_PREMIUM,
        VS_UPPER,
        DL,
        OA
    }

    public enum PassengersCheckInStatus
    {
        NOT_AVAILABLE,
        COMPLETED,
        PARTIALLY_COMPLETE,
        TO_COMPLETE
    }

    public enum LoggingCheckinEvent
    {
        CHECKIN_SESSION_REQUESTED,
        CHECKIN_SESSION_CREATED,
        CHECKIN_END_SUCCESS,
        CHECKIN_END_REFUSED,
        CHECKIN_PASSENGERS_REQUESTED,
        CHECKIN_PASSENGERS_SELECTED,
        PASSPORT_DETAILS_SUBMITTED,
        PASSPORT_DETAILS_SAVED,
        RETURN_DATE_SUBMITTED,
        RETURN_DATE_SAVED,
        EMERGENCY_CONTACT_SUBMITTED,
        EMERGENCY_CONTACT_SAVED,
        CONTACT_TRACING_SUBMITTED,
        CONTACT_TRACING_SAVED,
        AUTO_SEAT_ASSIGNED,
        DAY_OF_TRAVEL,
        BOARDING_PASS_FAILED,
        BAGGAGE_OPTIONS_REQUESTED,
        BAGGAGE_OPTIONS_RECEIVED,
        BAGGAGE_DETAILS_SUBMITTED,
        BAGGAGE_DETAILS_SAVED,
        BAGGAGE_DETAILS_FAILED,
        BAGGAGE_PAYMENT_ELIGIBILITY_INITIATED,
        PAYMENT_ELIGIBILITY_CHECKED,
        PAYMENT_SESSION_CREATED,
        CONFIRM_BAGGAGE_PAYMENT_TRANSACTION_SUBMITTED,
        CONFIRM_BAGGAGE_PAYMENT_REVIEW_SUBMITTED,
    }

    public enum LoggingFailureContext
    {
        INITIATE_CHECKIN,
        SELECT_PASSENGER,
        SAVE_RETURN_DATE,
        SAVE_PASSPORT,
        SAVE_EMERGENCY_CONTACT,
        SAVE_CONTACT_TRACING,
        SAVE_GOVERNMENT_FORM,
        SAVE_SECONDARY_DOCUMENT,
        BOARDING_PASS_GENERATION,
        BAGGAGE_OFFERS_RETRIEVAL,
        SAVE_BAGGAGE,
        PAYMENT_ELIGIBILITY_CHECK,
        CONFIRM_BAGGAGE,
        GET_TRAVEL_REQUIREMENTS,
        GET_SECURE_FLIGHT_INFO,
        UPDATE_SECURE_FLIGHT_INFO
    }

    public enum PaymentType
    {
        [EnumMember(Value = "Card")]
        Card = 1,
        [EnumMember(Value = "Voucher")]
        Voucher = 2,
        [EnumMember(Value = "Wallet")]
        Wallet = 3,
        [EnumMember(Value = "APM")]
        APM = 4,
        [EnumMember(Value = "CardToken")]
        CardToken = 5,
        [EnumMember(Value = "Virtual")]
        Virtual = 6,
        [EnumMember(Value = "OnlineBanking")]
        OnlineBanking = 7,
        [EnumMember(Value = "ApplePay")]
        ApplePay = 8,
        [EnumMember(Value = "PayPal")]
        PayPal = 9,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardTypes
    {
        VI = 0,
        CA = 1,
        AX = 2,
        TP = 3,
        DC = 4,
        DS = 5,
        TO = 6,

        Visa = VI,
        MasterCard = CA,
        AmericanExpress = AX,
        UATP = TP,
        Diners = DC,
        Discover = DS,
        Maestro = TO

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum BasketStatusEnum
    {
        [EnumMember(Value = "FAILED")]
        FAILED,

        [EnumMember(Value = "SUCCESS")]
        SUCCESS,

        [EnumMember(Value = "COMPLETED")]
        COMPLETED
    }

    public enum LoggingTravelRequirementsEvent
    {
        TRAVEL_REQUIREMENTS_SESSION_REQUESTED,
        TRAVEL_REQUIREMENTS_SESSION_CREATED,
        TRAVEL_REQUIREMENTS_SESSION_FAILED,
        TRAVEL_REQUIREMENTS_PASSPORT_DETAILS_SUBMITTED,
        TRAVEL_REQUIREMENTS_PASSPORT_DETAILS_SAVED,
        TRAVEL_REQUIREMENTS_RETURN_DATE_SUBMITTED,
        TRAVEL_REQUIREMENTS_RETURN_DATE_SAVED,
        TRAVEL_REQUIREMENTS_CONTACT_TRACING_SUBMITTED,
        TRAVEL_REQUIREMENTS_CONTACT_TRACING_SAVED,
        TRAVEL_REQUIREMENTS_EMERGENCY_CONTACT_SUBMITTED,
        TRAVEL_REQUIREMENTS_EMERGENCY_CONTACT_SAVED,
        TRAVEL_REQUIREMENTS_GOVERNMENT_FORM_SUBMITTED,
        TRAVEL_REQUIREMENTS_GOVERNMENT_FORM_SAVED,
        TRAVEL_REQUIREMENTS_SECONDARY_DOCUMENT_SUBMITTED,
        TRAVEL_REQUIREMENTS_SECONDARY_DOCUMENT_SAVED,
        TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_REQUESTED,
        TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_RETRIEVED,
        TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_FAILED,
        TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_UPDATE_INITIATED,
        TRAVEL_REQUIREMENTS_SECURE_FLIGHT_INFO_UPDATED
    }

    public enum FlightStatusCode
    {
        UNKNOWN,
        ON_TIME,
        DELAYED,
        CANCELLED
    }
}
