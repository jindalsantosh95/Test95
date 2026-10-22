using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions
{
    public class CustomerTransactionRepoCommand : IRequest<Result<Unit>>
    {
        public CosmosDocumentEvent Event { get; }
        public CustomerTransactionRepoCommand(CosmosDocumentEvent cosmosEvent)
        {
            Event = cosmosEvent;
        }

    }
    public class CustomerTransactionRepoCommandHandler : IRequestHandler<CustomerTransactionRepoCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerTransactionRepoCommandHandler> _logger;

        public CustomerTransactionRepoCommandHandler(IMediator mediator, ILogger<CustomerTransactionRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(CustomerTransactionRepoCommand request, CancellationToken cancellationToken)
        {
            CosmosModifyRepoCommand command = new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDB),
                CollectionName = ConfigReader.Get(Constants.EnvironmentSettings.TransactionDBCollection),
                Event = request.Event,
            };

            await _mediator.Send(command, cancellationToken);
            return Unit.Value;
        }
    }
}
