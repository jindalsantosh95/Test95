using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Email;

namespace VAA.EAPI.Infrastructure.Email.Interface
{
    public interface ICcpRepository
    {
        Task<bool> SendEmail(SendEmailRequest request);
    }
}
