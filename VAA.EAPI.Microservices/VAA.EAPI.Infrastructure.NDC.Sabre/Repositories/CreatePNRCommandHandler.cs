using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class CreatePNRCommandHandler(ICreatePNR createPNR) : IRequestHandler<CreatePNRCommand, CreatePassengerNameRecordRS>
    {
        private readonly ICreatePNR _createPNR = createPNR ?? throw new ArgumentNullException(nameof(createPNR));

        public async Task<CreatePassengerNameRecordRS> Handle(CreatePNRCommand request, CancellationToken cancellationToken)
        {
            return await _createPNR.CreatePNRAsync(request.CreatePNRRequest);
        }
    }
}
