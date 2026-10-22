using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Email;

namespace VAA.EAPI.Infrastructure.Email.Interface
{
    public interface ICommunicationRepository
    {
        Task<bool> Notify(SendEmailRequest request, string CorrelationId, CancellationToken cancellationToken);
    }
}
