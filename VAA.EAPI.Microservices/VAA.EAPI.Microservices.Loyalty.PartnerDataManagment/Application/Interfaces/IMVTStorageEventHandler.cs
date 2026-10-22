using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface IMVTStorageEventHandler
    {
        Task<string> HandleAsync(MVTMessageRequest message);
    }
}
