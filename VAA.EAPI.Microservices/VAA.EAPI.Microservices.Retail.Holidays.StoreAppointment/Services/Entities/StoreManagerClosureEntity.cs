namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities;

public class StoreManagerClosureEntity
{
    public string ClosureId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public DateOnly ClosureDate { get; set; }
    public string ClosureType { get; set; } = string.Empty;
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}
