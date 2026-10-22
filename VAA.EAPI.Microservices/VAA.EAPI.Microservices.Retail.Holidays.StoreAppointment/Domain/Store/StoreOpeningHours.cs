namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreOpeningHours
{
    public bool IsOpenNow { get; set; }
    public IEnumerable<StoreOpeningWindow> WeeklyHours { get; set; } = Enumerable.Empty<StoreOpeningWindow>();
    public IEnumerable<StoreExceptionalOpening> SpecialHours { get; set; } = Enumerable.Empty<StoreExceptionalOpening>();
}
