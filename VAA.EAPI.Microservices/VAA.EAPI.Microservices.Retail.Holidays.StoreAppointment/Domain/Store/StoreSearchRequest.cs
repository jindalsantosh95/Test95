namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreSearchRequest
{
    public string SearchParameter { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public DateTime SelectedDate { get; set; } = DateTime.MinValue;
    public int Year { get; set; }
    public int Month { get; set; }
}
