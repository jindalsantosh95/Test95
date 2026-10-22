using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository.ChangeFeed
{
    public class TicketRepoCommandHandler : IRequestHandler<TicketFeedRepoCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TicketRepoCommandHandler> _logger;

        public TicketRepoCommandHandler(IMediator mediator, ILogger<TicketRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(TicketFeedRepoCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.TicketCosmosDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.TicketCosmosDBName),
                CollectionName = ConfigReader.Get(Constants.Environment.TicketCosmosDBCollection),
                Event = request.Event,
            };

            await _mediator.Send(command, cancellationToken);

            return Unit.Value;
        }
    }
}
