using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class StoreClosureRequest
{
    public LocalDate ClosureDate { get; set; }
    public string ClosureType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
