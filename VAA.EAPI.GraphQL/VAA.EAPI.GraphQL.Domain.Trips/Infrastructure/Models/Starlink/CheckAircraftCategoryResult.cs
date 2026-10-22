namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.Starlink;

public sealed class CheckAircraftCategoryResult
{
    public string TailNumber { get; set; }
    public bool DoesCategoryExist { get; set; }
}
