using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.CRM.Email.Domain;

namespace VAA.EAPI.Microservices.CRM.Email.Application.Command
{
    public interface ISendOrderEmailCommand
    {
        Task Handle(SendOrderEmailRequest request, CancellationToken cancellationToken);
    }
}