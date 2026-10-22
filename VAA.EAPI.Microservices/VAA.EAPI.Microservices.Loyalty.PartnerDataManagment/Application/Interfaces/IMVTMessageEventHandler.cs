using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface IMVTMessageEventHandler
    {
        Task<bool> Handle(MVTMessageRequest message);
    }
}
