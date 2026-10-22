using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FareRules
{
    public class FareRuleQuery : IRequest<Result<Response<OfferFareRules>>>
    {
        public string OfferId { get; set; }
        public string Type { get; set; }

    }
}
