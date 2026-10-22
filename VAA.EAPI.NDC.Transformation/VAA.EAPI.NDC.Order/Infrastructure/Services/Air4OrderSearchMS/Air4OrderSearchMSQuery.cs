using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderSearchMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.Air4OrderSearchMS
{
    public class Air4OrderSearchMSQuery : IRequest<Air4OrderSearchMSResponse>
    {
        public OrderSearchRequest OrderSearchRequest { get; set; }
    }
}
