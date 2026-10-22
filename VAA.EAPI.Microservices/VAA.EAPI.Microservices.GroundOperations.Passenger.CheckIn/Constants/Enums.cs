namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Constants
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
    public enum DocumentType
    {
        UNKNOWN, // Default / null value
        PASSPORT,
        VISA,
        BRP,
        PERMANENT_RESIDENT_CARD,
        NATIONAL_ID,
        NEXUS_CARD,
        ETA,
        TRANSIT_VISA
    }

    public enum PassportVerificationStatus
    {
        UNKNOWN, // Default / null value
        NOT_VERIFIED,
        VERIFIED
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

    public enum ApplicationType
    {
        Vendor,
        Government
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

    public enum FlightStatusCode
    {
        UNKNOWN,
        ON_TIME,
        DELAYED,
        CANCELLED
    }

    public enum PassengersCheckInStatus
    {
        NOT_AVAILABLE,
        COMPLETED,
        PARTIALLY_COMPLETE,
        TO_COMPLETE
    }
}
