using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData;
using VAA.EAPI.Infrastructure.Mapper.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;


namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository
{
    public class RetriveTicketRepoHandler : IRequestHandler<RetriveTicketRepoQuery, TicketData>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RetriveTicketRepoHandler> _logger;

        public RetriveTicketRepoHandler(IMediator mediator, ILogger<RetriveTicketRepoHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TicketData> Handle(RetriveTicketRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                CosmosReaderRepoQuery objQuery = new()
                {
                    CollectionName = ConfigReader.Get(Constants.Environment.TicketCosmosDBCollection),
                    DatabaseEndpoint = ConfigReader.Get(Constants.Environment.TicketCosmosDBEndPoint),
                    DatabaseName = ConfigReader.Get(Constants.Environment.TicketCosmosDBName),
                    SqlQuery = Constants.Application.TicketRetrivalSelectQueryForSkyTeam,
                    QueryParams = new() { new KeyValuePair<string, object>("@TicketNumber", request.RetriveTicketRequest.ETicketNumber) },

                };

                CosmosReaderServiceQuery cosmosReaderServiceCommand = CommandMapper.MapCosmosReaderCommand(objQuery);

                var ticketDocResponse = await _mediator.Send(cosmosReaderServiceCommand, cancellationToken);

                var primaryDoc = ticketDocResponse.Any() ? Json.DeserializeObject<TicketData>((JToken)ticketDocResponse.FirstOrDefault()) : new();

                if (request.RetriveTicketRequest.IncludeAssociatedEmds)
                {
                    var attachedEmds = new List<TicketData>();
                    CosmosReaderRepoQuery assocEmdsQuery = new()
                    {
                        CollectionName = ConfigReader.Get(Constants.Environment.TicketCosmosDBCollection),
                        DatabaseEndpoint = ConfigReader.Get(Constants.Environment.TicketCosmosDBEndPoint),
                        DatabaseName = ConfigReader.Get(Constants.Environment.TicketCosmosDBName),
                        SqlQuery = Constants.Application.AssocEmdsRetrivalSelectQueryForSkyTeam,
                        QueryParams = new() { new KeyValuePair<string, object>("@TicketNumber", request.RetriveTicketRequest.ETicketNumber) },
                    };

                    CosmosReaderServiceQuery cosmosReaderCmd = CommandMapper.MapCosmosReaderCommand(assocEmdsQuery);

                    var ticketEmdResponse = await _mediator.Send(cosmosReaderCmd, cancellationToken);

                    primaryDoc.AttachedEmdDocuments = ticketEmdResponse.Select(t => Json.DeserializeObject<TicketData>(((JToken)t).FirstOrDefault().FirstOrDefault())).ToArray();
                }

                return primaryDoc;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RetriveTicketRepoHandler failed with request : {Json.Serialize(request)} exceptions {ex.Message}");
                throw;
            }
        }
    }
}
