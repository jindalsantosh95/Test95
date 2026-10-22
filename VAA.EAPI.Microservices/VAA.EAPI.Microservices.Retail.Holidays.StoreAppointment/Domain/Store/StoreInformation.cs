namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreInformation
{
    public StoreContactDetails Details { get; set; } = new();
    public StoreOpeningHours OpeningHours { get; set; } = new();
    public StoreServiceList Services { get; set; } = new();
}
