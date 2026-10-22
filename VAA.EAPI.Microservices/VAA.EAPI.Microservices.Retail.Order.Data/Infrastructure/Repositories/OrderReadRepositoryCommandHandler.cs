using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class OrderReadRepositoryCommandHandler : IRequestHandler<OrderReadRepositoryCommand, PnrSearch>
    {
        private readonly ILogger<OrderReadRepositoryCommandHandler> _logger;
        private readonly IMediator _mediator;

        public OrderReadRepositoryCommandHandler(IMediator mediator, ILogger<OrderReadRepositoryCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<PnrSearch> Handle(OrderReadRepositoryCommand request, CancellationToken cancellationToken)
        {
            var partitionKey = $"{request?.Req?.Pnr}{"_"}{request?.Req?.IssueDate.Replace("-", string.Empty)}";
            var query = new CosmosReaderRepoQuery()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.OrderDBEndpoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.DatabaseName),
                CollectionName = ConfigReader.Get(Constants.Environment.OrderCollection),
                SqlQuery = Constants.Application.Query,
                QueryParams = new()
                {
                    new KeyValuePair<string, object>("PartitionKey", partitionKey),
                    new KeyValuePair<string, object>("DocType", "PnrSearch")
                }
            };

            List<object> cosmosDocs;
            try
            {
                cosmosDocs = await _mediator.Send(query, CancellationToken.None);
            }
            catch
            {
                var error = new Response<string>()
                {
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.CosmosErorrCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                }; throw new CosmosDBException($"{Json.Serialize(error.Errors)}");
            }

            if (cosmosDocs.Any())
            {
                return cosmosDocs.Select(doc => Json.DeserializeObject<PnrSearch>((JToken)doc)).First();
            }
            else
            {
                _logger.LogWarning("Order Read PNR search document was not found in Cosmos DB for PNR {Pnr} and correlation {CorrelationId}",
                    request?.Req?.Pnr, AsyncContext.Current?.CorrelationId);
                return new();
            }

        }
    }
}
