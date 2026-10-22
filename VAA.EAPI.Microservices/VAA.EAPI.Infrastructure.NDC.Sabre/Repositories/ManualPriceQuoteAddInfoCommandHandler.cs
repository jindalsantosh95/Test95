using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class ManualPriceQuoteAddInfoCommandHandler(IManualPriceQuoteAddInfo manualPriceQuoteAddInfo) : IRequestHandler<ManualPriceQuoteAddInfoCommand, ManualPriceQuoteAddInfoRS>
    {
        private readonly IManualPriceQuoteAddInfo _manualPriceQuoteAddInfo= manualPriceQuoteAddInfo ?? throw new ArgumentNullException(nameof(manualPriceQuoteAddInfo));

        public async Task<ManualPriceQuoteAddInfoRS> Handle(ManualPriceQuoteAddInfoCommand request, CancellationToken cancellationToken)
        {
            return await _manualPriceQuoteAddInfo.ManualPriceQuoteAddInfoRQAsync(request.ManualPriceQuoteRequest);
        }
    }
}
