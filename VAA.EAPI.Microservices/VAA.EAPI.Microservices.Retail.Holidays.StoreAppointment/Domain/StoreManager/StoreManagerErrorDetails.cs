using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public sealed class StoreManagerErrorDetails : ErrorDetails
{
    public IReadOnlyDictionary<string, object?>? Details { get; set; }
}
