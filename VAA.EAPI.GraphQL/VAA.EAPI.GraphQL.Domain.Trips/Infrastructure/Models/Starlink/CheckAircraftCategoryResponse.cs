namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Starlink;

public sealed class CheckAircraftCategoryResponse
{
    public IEnumerable<CheckAircraftCategoryResult> AircraftCategoryChecks { get; set; } = [];
}
