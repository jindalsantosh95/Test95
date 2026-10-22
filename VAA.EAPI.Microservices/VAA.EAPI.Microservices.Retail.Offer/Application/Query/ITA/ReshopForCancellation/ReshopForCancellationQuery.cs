using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation
{
    public class ReshopForCancellationQuery : ReshopRequest, IRequest<Result<Response<CancelReshopResponse>>>
    {

    }
}
