using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.CancelReshop
{
    public class CancelReshopQuery : IRequest<Result<Response<CancelReshopResponse>>>
    {
        public CancelReshopRequest CancelOrderRequest;

        public CancelReshopQuery(CancelReshopRequest cancelOrderRequest)
        {
            CancelOrderRequest = cancelOrderRequest;
        }
    }
}
