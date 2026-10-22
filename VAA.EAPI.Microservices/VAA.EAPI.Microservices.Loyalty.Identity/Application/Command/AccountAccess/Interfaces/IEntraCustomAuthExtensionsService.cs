using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces
{
    public interface IEntraCustomAuthExtensionsService
    {
        Task<Response<ResponseObject>> CustomToken(EntraRequest entraCode);
        Task<Response<ResponseObject>> CustomEmailOtp(EntraRequest entraOtpequest);
    }
}
