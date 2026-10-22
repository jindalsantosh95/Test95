using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces
{
    public interface IPartnerDataService
    {
        Task<IReadOnlyCollection<Partner>> GetSkyTeamPartnersAsync(CancellationToken cancellationToken);
    }
}
