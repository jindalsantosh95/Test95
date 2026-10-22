namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

internal sealed class StoreManagerSlotRow
{
    public int StoreSlotId { get; set; }
    public string? StoreId { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public int Capacity { get; set; }
    public int RemainingCapacity { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
}
