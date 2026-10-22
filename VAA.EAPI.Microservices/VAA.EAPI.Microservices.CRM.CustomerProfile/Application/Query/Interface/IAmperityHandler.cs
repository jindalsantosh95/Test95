using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Query.Interface
{
    public interface IAmperityHandler
    {
        Task<Response<AmperityDetails>> GetAmperityDetails(string loyaltyId);
    }
}
