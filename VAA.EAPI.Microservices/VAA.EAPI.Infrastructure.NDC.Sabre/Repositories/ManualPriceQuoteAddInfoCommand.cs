using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.ManualPriceQuoteAddInfoLLS;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class ManualPriceQuoteAddInfoCommand : IRequest<ManualPriceQuoteAddInfoRS>
    {
        public ManualPriceQuoteAddInfoRQRequest ManualPriceQuoteRequest { get; set; }
    }
}