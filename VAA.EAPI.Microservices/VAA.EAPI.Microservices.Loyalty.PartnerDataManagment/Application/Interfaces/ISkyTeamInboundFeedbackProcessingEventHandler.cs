using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface ISkyTeamInboundFeedbackProcessingEventHandler
    {
        Task<bool> Handle(AutoAccrualFeedbackProcessingPayload message, CancellationToken cancellationToken);
    }
}
