using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.Air4OrderSearchMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.Air4OrderSearchMS
{
    public class Air4OrderSearchMSQuery : IRequest<Air4OrderSearchMSResponse>
    {
        public OrderSearchRequest OrderSearchRequest { get; set; }
    }
}
