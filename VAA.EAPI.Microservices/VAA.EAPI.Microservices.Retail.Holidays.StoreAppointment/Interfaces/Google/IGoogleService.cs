using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Google;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Google;

public interface IGoogleService
{
    Task<IReadOnlyDictionary<string, DistanceMatrixResult>> GetDistanceMetricsAsync(string origin, IReadOnlyCollection<StoreResult> stores);
}
