using MediatR;
using VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.OrderReshop
{
    public class OrderReshopBundleQuery : OrderReshopRequest, IRequest<OfferResponse>
    {
    }
}