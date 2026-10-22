using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.Reprice
{
    public class OfferRepriceQuery : RepriceRequest, IRequest<Result<Response<OfferResponse>>>
    {

    }
}
