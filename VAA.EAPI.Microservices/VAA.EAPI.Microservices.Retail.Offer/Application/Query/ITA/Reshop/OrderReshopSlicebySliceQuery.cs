
using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reshop
{
    public class OrderReshopSlicebySliceQuery : ReshopRequest, IRequest<Result<Response<OfferResponse>>>
    {

    }
}
