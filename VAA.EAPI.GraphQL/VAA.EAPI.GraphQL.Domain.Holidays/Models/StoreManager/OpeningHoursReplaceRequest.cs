namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class OpeningHoursReplaceRequest
{
    public IEnumerable<OpeningHoursEntryRequest> OpeningHours { get; set; } = Enumerable.Empty<OpeningHoursEntryRequest>();
}

