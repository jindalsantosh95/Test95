using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces
{
    public interface IOTPGenerateCommandHandler
    {
        Task<Response<string>> Handle(OTPGenerateCommand request, CancellationToken cancellationToken);
    }
}
