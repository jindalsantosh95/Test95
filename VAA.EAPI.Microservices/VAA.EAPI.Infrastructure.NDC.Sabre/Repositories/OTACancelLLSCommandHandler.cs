using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.OTACancel;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class OTACancelLLSCommandHandler : IRequestHandler<OTACancelLLSCommand, OTA_CancelRQResponse>
    {
        private readonly IOTACancelLLS _otaCancel;

        public OTACancelLLSCommandHandler(IOTACancelLLS otaCancel)
        {
            _otaCancel = otaCancel ?? throw new ArgumentException(nameof(otaCancel));
        }

        public async Task<OTA_CancelRQResponse> Handle(OTACancelLLSCommand request, CancellationToken cancellationToken)
        {
            return await _otaCancel.OTACancelLLSAsync(request.OtaCancelRequest);
        }
    }
}
