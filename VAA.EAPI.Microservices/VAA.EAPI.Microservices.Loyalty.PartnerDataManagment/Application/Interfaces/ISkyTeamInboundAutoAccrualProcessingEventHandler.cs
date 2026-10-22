using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface ISkyTeamInboundAutoAccrualProcessingEventHandler
    {
        Task<bool> Handle(AutoAccrualProcessingPayload processingPayload, CancellationToken cancellationToken);
    }
}
