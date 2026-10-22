using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IOTPMutationProvider<T>
    {
        Task<T> GenerateOtp(string emailId);

        Task<T> GenerateOtpContinuationToken(string emailId);

        Task<T> ValidateOtpToken(string token);

        Task<T> ValidateOtp(string emailId, int otp);
        Task<T> GenerateEntraOtp(string emailId);
    }
}
