using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Mapper.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;

namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2
{
    public class CosmosModifyRepoCommand : CosmosRepoBase, IRequest<Result<Unit>>
    {
        public CosmosDocumentEvent Event { get; set; }

    }
    public class CosmosModifyRepoCommandHandler : IRequestHandler<CosmosModifyRepoCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CosmosModifyRepoCommandHandler> _logger;

        public CosmosModifyRepoCommandHandler(IMediator mediator, ILogger<CosmosModifyRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(CosmosModifyRepoCommand cosmosModifyRepoCommand, CancellationToken cancellationToken)
        {
            CosmosModifyServiceCommand cosmosModifyServiceCommand = CommandMapper.MapCosmosModifyCommand(cosmosModifyRepoCommand);

            await _mediator.Send(cosmosModifyServiceCommand, cancellationToken);

            return Unit.Value;
        }
    }
}
