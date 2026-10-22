namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreContactDetails
{
    public string Phone { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public StoreAddress Address { get; set; } = new();
}
