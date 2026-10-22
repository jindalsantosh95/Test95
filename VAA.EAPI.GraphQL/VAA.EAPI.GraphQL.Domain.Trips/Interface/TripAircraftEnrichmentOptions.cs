namespace VAA.EAPI.GraphQL.Domain.Trips.Interface;

public sealed record TripAircraftEnrichmentOptions(bool IncludeAircraftTailNumber, bool IncludeStarlinkEligibility)
{
    public bool RequiresOperationalSchedule => IncludeAircraftTailNumber || IncludeStarlinkEligibility;
}
