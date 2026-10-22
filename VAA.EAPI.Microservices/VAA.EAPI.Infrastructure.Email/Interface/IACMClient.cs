using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Email.Interface
{
    public interface IACMClient
    {
        Task<bool> SendEmail(string aCMRequest);
    }
}
