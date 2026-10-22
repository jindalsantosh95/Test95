namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

internal sealed class StoreManagerClosureRow
{
    public string? ClosureId { get; set; }
    public string? StoreId { get; set; }
    public DateOnly ClosureDate { get; set; }
    public string? ClosureType { get; set; }
    public string? Note { get; set; }
}