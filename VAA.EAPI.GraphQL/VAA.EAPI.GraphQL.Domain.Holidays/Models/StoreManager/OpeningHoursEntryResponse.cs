using InfrastructureStoreManagerOpeningHoursEntryResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.OpeningHoursEntryResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class OpeningHoursEntryResponse
{
    public int DayOfWeek { get; set; }
    public string OpensAt { get; set; } = string.Empty;
    public string ClosesAt { get; set; } = string.Empty;

    public static explicit operator OpeningHoursEntryResponse(InfrastructureStoreManagerOpeningHoursEntryResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new OpeningHoursEntryResponse
        {
            DayOfWeek = response.DayOfWeek,
            OpensAt = response.OpensAt,
            ClosesAt = response.ClosesAt
        };
    }
}
