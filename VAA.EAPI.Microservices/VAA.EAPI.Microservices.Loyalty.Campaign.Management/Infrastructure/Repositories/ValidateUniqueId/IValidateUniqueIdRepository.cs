using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Infrastructure.Repositories.ValidateUniqueId.Interface
{
    public interface IValidateUniqueIdRepository
    {
        Task<CampaignUniqueIdTableEntity> GetByUniqueIdAsync(string uniqueId);
    }
}
