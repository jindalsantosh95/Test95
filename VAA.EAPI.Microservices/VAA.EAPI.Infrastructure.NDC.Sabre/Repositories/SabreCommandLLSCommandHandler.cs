using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SabreCommandLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class SabreCommandLLSCommandHandler : IRequestHandler<SabreCommandLLSCommand , SabreCommandLLSRS>
    {
        private readonly ISabreCommandLLS _sabreCommandLLS;

        public SabreCommandLLSCommandHandler(ISabreCommandLLS sabreCommandLLS)
        {
            _sabreCommandLLS = sabreCommandLLS ?? throw new ArgumentException(nameof(sabreCommandLLS));
        }

        public async Task<SabreCommandLLSRS> Handle(SabreCommandLLSCommand request, CancellationToken cancellationToken)
        {
            return await _sabreCommandLLS.SabreCommandLLSAsync(request.SabreCommandLLSRequest);
        }
    }
}
