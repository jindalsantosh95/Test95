using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface IAutoAccrualFeedbackOutboundEventHandler
    {
        Task<bool> Handle(DistanceBasedAccrualFeedbackPayload message);
    }
}
