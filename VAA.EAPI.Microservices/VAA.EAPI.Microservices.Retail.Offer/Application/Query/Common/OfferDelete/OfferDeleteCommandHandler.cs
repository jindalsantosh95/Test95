using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Common.OfferDelete
{
    public class OfferDeleteCommandHandler : IRequestHandler<OfferDeleteCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public OfferDeleteCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Result<Unit>> Handle(OfferDeleteCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(new OfferDeleteRepoCommand(Json.DeserializeObject<CosmosDocument>((JToken)request.Data)), cancellationToken);

            return Unit.Value;
        }
    }
}
