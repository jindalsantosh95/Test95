using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Delta.FareRules
{
    public class FareRuleQueryHandler : IRequestHandler<FareRuleQuery, Result<Response<OfferFareRules>>>
    {
        private readonly IDeltaOfferService _deltaOfferService;

        public FareRuleQueryHandler(IDeltaOfferService deltaOfferService)
        {
            _deltaOfferService = deltaOfferService ?? throw new ArgumentNullException(nameof(deltaOfferService));
        }

        public async Task<Result<Response<OfferFareRules>>> Handle(FareRuleQuery request, CancellationToken cancellationToken)
        {
            return await _deltaOfferService.GetOfferFareRules(request.OfferId.Split(Constants.Application.Pipeline).Last());
        }
    }
}
