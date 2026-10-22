using InfrastructureStoreManagerWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.StoreManagerWindowResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class StoreManagerWindowResponse
{
    public string Id { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public int Capacity { get; set; }
    public int RemainingCapacity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }

    public static explicit operator StoreManagerWindowResponse(InfrastructureStoreManagerWindowResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new StoreManagerWindowResponse
        {
            Id = response.Id,
            StoreId = response.StoreId,
            StartAt = response.StartAt,
            EndAt = response.EndAt,
            Capacity = response.Capacity,
            RemainingCapacity = response.RemainingCapacity,
            Status = response.Status,
            Reason = response.Reason
        };
    }
}
