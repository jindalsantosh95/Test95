using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces
{
    public interface IOTPGenerateContinuationTokenCommandHandler
    {
        Task<Response<string>> Handle(OTPGenerateContinuationTokenCommand request, CancellationToken cancellationToken);
    }
}
