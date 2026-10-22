namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class NearbyStoresResponse
{
    public IEnumerable<StoreRecord> Stores { get; set; } = Enumerable.Empty<StoreRecord>();
}