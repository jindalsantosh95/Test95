namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreWorkHoursPayload
{
    public IReadOnlyCollection<StoreWorkHours> WeeklyHours { get; set; } = Array.Empty<StoreWorkHours>();
    public IReadOnlyCollection<StoreSpecialHours> SpecialHours { get; set; } = Array.Empty<StoreSpecialHours>();
}