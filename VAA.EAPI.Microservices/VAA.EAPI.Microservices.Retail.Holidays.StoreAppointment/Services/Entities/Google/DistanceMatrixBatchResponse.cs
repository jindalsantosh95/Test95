using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities.Google;


public partial class GoogleService
{
    private sealed record DistanceMatrixBatchResponse(StoreResult[] Stores, GoogleDistanceMatrixRow? Row);
}