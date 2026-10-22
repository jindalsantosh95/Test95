namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class OpeningHoursReplaceRequest
{
    public IEnumerable<OpeningHoursEntryRequest> OpeningHours { get; set; } = Enumerable.Empty<OpeningHoursEntryRequest>();
}
