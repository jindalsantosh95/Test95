using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.BrandMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS
{
    public class BrandMSCommand : BrandQueryRequest, IRequest<BrandRS>
    {
    }
}
