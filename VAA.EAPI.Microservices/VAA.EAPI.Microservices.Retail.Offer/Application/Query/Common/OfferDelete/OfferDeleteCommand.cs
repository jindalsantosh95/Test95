using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.OfferDelete
{
    public class OfferDeleteCommand : CloudEvent, IRequest<Result<Unit>>
    {
    }
}
