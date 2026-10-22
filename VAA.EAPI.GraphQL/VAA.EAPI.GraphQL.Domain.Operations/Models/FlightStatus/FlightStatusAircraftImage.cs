#nullable enable
namespace VAA.EAPI.GraphQL.Domain.Operations.Models.FlightStatus;

public sealed class FlightStatusAircraftImage
{
    public string? DesktopUrl { get; init; }
    public string? MobileUrl { get; init; }

    public static explicit operator FlightStatusAircraftImage(FlightStatusInternalAircraftImage source) => new()
    {
        DesktopUrl = source.DesktopUrl,
        MobileUrl = source.MobileUrl
    };
}
