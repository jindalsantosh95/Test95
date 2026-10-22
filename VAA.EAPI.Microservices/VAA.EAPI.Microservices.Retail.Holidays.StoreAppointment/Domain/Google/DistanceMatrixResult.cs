namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Google;

public sealed class DistanceMatrixResult
{
    public const long UnknownDistanceInMeters = long.MaxValue;
    public long DistanceInMeters { get; set; }
    public string DistanceText { get; set; } = string.Empty;
    public string DurationText { get; set; } = string.Empty;
}
