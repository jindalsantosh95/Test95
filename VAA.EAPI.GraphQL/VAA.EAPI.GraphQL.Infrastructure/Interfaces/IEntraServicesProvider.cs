using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IEntraServicesProvider<T>
    {
        Task<T> GenerateRefreshToken(RefreshTokenRequest request);
        Task<T> PasswordResetStart(PasswordResetStartRequest request);
        Task<T> EntraOtpValidation(EntraOtpValidationRequest request);
        Task<T> NewPasswordSubmission(NewPasswordSubmissionRequest request);
    }
}
