namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreDayAvailability
{
    public DateTime Date { get; set; }
    public bool HasAvailableSlot { get; set; }
}