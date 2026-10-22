using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Data.Egress.Application.Command
{
    public class AirportsPublishCommandHandler : IRequestHandler<AirportsPublishCommand, bool>
    {
        public async Task<bool> Handle(AirportsPublishCommand request, CancellationToken cancellationToken)
        {
            return true;
        }
    }
}
