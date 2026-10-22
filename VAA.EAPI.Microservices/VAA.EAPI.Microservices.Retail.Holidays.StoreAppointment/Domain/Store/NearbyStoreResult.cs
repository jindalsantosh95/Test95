namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class NearbyStoreResult
{
    public IEnumerable<StoreResult> Stores { get; set; } = Enumerable.Empty<StoreResult>();
}
