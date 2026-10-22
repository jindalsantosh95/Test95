using InfraResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.NearbyStoresResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator
{
    public class NearbyStoresResponse
    {
        public IEnumerable<StoreRecord> Stores { get; set; } = Enumerable.Empty<StoreRecord>();

        public static explicit operator NearbyStoresResponse(InfraResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            return new NearbyStoresResponse
            {
                Stores = response.Stores.Select(s => (StoreRecord)s)
            };
        }
    }
}