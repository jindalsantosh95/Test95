namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreExceptionalOpening
{
    public string Label { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string OpeningTime { get; set; } = string.Empty;
    public string ClosingTime { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
}
