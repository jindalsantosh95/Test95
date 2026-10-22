namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreSpecialHoursPayload
{
    public IReadOnlyCollection<StoreSpecialHours> SpecialHours { get; set; } = Array.Empty<StoreSpecialHours>();
}
