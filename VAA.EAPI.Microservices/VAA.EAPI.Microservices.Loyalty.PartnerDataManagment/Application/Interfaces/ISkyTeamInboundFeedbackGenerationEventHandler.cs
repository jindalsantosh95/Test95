using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface ISkyTeamInboundFeedbackGenerationEventHandler
    {
        Task<bool> Handle(AutoAccrualFeedbackGenerationPayload message, CancellationToken cancellationToken);
    }
}
