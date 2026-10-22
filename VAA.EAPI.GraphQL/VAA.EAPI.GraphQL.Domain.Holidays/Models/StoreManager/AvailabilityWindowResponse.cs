using InfrastructureStoreManagerAvailabilityWindowResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.AvailabilityWindowResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class AvailabilityWindowResponse
{
    public string Id { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public int Capacity { get; set; }
    public int RemainingCapacity { get; set; }

    public static explicit operator AvailabilityWindowResponse(InfrastructureStoreManagerAvailabilityWindowResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new AvailabilityWindowResponse
        {
            Id = response.Id,
            StoreId = response.StoreId,
            StartAt = response.StartAt,
            EndAt = response.EndAt,
            Capacity = response.Capacity,
            RemainingCapacity = response.RemainingCapacity
        };
    }
}
