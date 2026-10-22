using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface ISkyTeamAutoAccrualEventHandler
    {
        Task<bool> Handle(AutoAccrualPayload message, CancellationToken cancellationToken);
    }
}
