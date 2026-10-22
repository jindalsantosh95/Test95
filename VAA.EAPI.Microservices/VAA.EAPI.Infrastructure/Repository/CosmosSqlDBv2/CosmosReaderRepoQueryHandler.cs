using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Mapper.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;

namespace VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2
{
    public class CosmosReaderRepoQuery : CosmosRepoBase, IRequest<List<object>>
    {
        public string SqlQuery { get; set; }
        public List<KeyValuePair<string, object>> QueryParams { get; set; }
    }
    public class CosmosReaderRepoQueryHandler : IRequestHandler<CosmosReaderRepoQuery, List<object>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CosmosReaderRepoQueryHandler> _logger;

        public CosmosReaderRepoQueryHandler(IMediator mediator, ILogger<CosmosReaderRepoQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<List<object>> Handle(CosmosReaderRepoQuery cosmosReaderRepoQuery, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"CosmosReaderRepoQueryHandler.Handle - Start");
            try
            {
                CosmosReaderServiceQuery cosmosReaderServiceCommand = CommandMapper.MapCosmosReaderCommand(cosmosReaderRepoQuery);

                var response = await _mediator.Send(cosmosReaderServiceCommand, cancellationToken);

                return response ?? new();
            }
            finally
            {
                _logger.LogInformation($"CosmosReaderRepoQueryHandler.Handle - End");
            }

        }
    }
}
