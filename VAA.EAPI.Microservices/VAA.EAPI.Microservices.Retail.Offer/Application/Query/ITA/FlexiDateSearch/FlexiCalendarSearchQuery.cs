using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FlexiDateSearch;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.FlexiDateSearch
{
    public class FlexiCalendarSearchQuery : OfferRequest, IRequest<Result<Response<FlexiDateSearchResponse>>>
    {

    }
}
