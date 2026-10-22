using NodaTime;
using InfrastructureStoreManagerStoreClosureResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreClosureResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class StoreClosureResponse
{
    public string Id { get; set; } = string.Empty;
    public LocalDate ClosureDate { get; set; }
    public string ClosureType { get; set; } = string.Empty;
    public string? Reason { get; set; }

    public static explicit operator StoreClosureResponse(InfrastructureStoreManagerStoreClosureResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new StoreClosureResponse
        {
            Id = response.Id,
            ClosureDate = LocalDate.FromDateTime(response.ClosureDate.ToDateTime(TimeOnly.MinValue)),
            ClosureType = response.ClosureType,
            Reason = response.Reason
        };
    }
}
