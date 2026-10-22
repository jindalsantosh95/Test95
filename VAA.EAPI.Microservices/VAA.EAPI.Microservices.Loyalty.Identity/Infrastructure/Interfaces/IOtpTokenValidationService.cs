using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IOtpTokenValidationService
    {
        Task<Response<string>> ValidaateOtpToken(OtpTokenValidationRequest request);
    }
}
