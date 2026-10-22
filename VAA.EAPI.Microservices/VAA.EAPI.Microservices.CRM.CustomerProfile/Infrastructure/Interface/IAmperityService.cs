using System.Threading.Tasks;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Interface
{
    public interface IAmperityService
    {
        Task<AmperityResponse> GetAmperityDetails(string loyaltyId);
    }
}
