using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Mapper.CosmosSqlDB;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDB;

namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDB
{
    [Obsolete("Use VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2")]
    public class CosmosReaderRepoCommandHandler : IRequestHandler<CosmosReaderRepoCommand, string>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CosmosReaderRepoCommandHandler> _logger;
        public CosmosReaderRepoCommandHandler(IMediator mediator, ILogger<CosmosReaderRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }
        public async Task<string> Handle(CosmosReaderRepoCommand cosmosReaderRepoCommand, CancellationToken cancellationToken)
        {
            CosmosReaderServiceCommand cosmosReaderServiceCommand = CommandMapper.MapCosmosReaderCommand(cosmosReaderRepoCommand);
            var response = await _mediator.Send(cosmosReaderServiceCommand, cancellationToken);
            return response != null ? Json.Serialize(response) : string.Empty;
        }
    }
}
