using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    public sealed record PnrScenario(
        LocationType Origin,
        LocationType Destination,
        JourneyType JourneyType,
        int Adults,
        int YoungAdults,
        int Children,
        int Infants,
        FlightType Airline,
        FareFamilyType FareFamily,
        CabinName CabinName,
        bool Seats,
        bool LoggedInUser,
        string PaymentType,
        bool Oci)
    {
        public string Name { get; init; } = "Unnamed";

        public string? StoragePartitionKey { get; init; }

        public string? PreferredTier { get; init; }

        public string? LeadFirstName { get; init; }

        public string? LeadLastName { get; init; }

        public int? DepartureOffsetDays { get; init; }

        public FareFamilyType? InboundFareFamily { get; init; }

        public FlightType? InboundAirline { get; init; }

        public string? RequiredLegOperatingAirlineCode { get; init; }

        public string? InboundRequiredLegOperatingAirlineCode { get; init; }

        public PnrOfferSelection OfferSelection { get; init; } = PnrOfferSelection.FareFamilyAndAirline;

        public int? LongLayoverHours { get; init; }

        public bool SameDayReturn { get; init; }

        public bool IncludeInsurance { get; init; }

        public IReadOnlyList<LocationType> MultiCityLocations { get; init; } = Array.Empty<LocationType>();

        public string? SeatProductName { get; init; }

        public bool UseSeasonalSchedule { get; init; }

        public string? FrequentFlyerProgramme { get; init; }

        public int? MinimumLoyaltyPoints { get; init; }
    }
}
