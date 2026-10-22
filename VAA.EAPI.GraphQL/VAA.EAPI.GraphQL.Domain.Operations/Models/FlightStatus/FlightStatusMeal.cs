#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusMeal
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? Available { get; init; }

    public static explicit operator FlightStatusMeal(FlightStatusInternalMeal source) => new()
    {
        Code = source.Code,
        Name = source.Name,
        Description = source.Description,
        Available = source.Available
    };
}
