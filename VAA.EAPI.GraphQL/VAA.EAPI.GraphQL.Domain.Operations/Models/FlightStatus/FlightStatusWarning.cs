#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusWarning
{
    public string? Code { get; init; }

    public static explicit operator FlightStatusWarning(FlightStatusInternalWarning source) => new()
    {
        Code = source.Code
    };
}
