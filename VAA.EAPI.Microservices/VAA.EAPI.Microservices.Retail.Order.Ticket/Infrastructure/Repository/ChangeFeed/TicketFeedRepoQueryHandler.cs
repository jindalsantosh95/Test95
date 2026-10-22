using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository.ChangeFeed
{
    public class TicketFeedRepoQueryHandler : IRequestHandler<TicketFeedRepoQuery, IEnumerable<object>>
    {
        protected readonly IMediator _mediator;
        protected readonly ILogger<TicketFeedRepoQueryHandler> _logger;

        public TicketFeedRepoQueryHandler(IMediator mediator, ILogger<TicketFeedRepoQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<object>> Handle(TicketFeedRepoQuery request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(ConfigureCosmosQuery(request), cancellationToken);
        }

        private CosmosReaderRepoQuery ConfigureCosmosQuery(TicketFeedRepoQuery request)
        {
            string sqlQuery = string.Empty;

            switch (request.QueryFilter)
            {
                case "FFPNumber":
                    sqlQuery = Constants.Application.TicketFFPNumberSearchSQL;
                    break;
                case "ContactEmail":
                    sqlQuery = Constants.Application.TicketEmailSearchSQL;
                    break;
                case "TicketNumber":
                    sqlQuery = Constants.Application.TicketNumberSearchSQL;
                    break;
            }

            return new()
            {
                DatabaseEndpoint = ConfigReader.Get(Constants.Environment.TicketCosmosDBEndPoint),
                DatabaseName = ConfigReader.Get(Constants.Environment.TicketCosmosDBName),
                CollectionName = ConfigReader.Get(Constants.Environment.TicketCosmosDBCollection),
                QueryParams = ConfigureQueryParams(request),
                SqlQuery = sqlQuery
            };
        }

        private List<KeyValuePair<string, object>> ConfigureQueryParams(TicketFeedRepoQuery request)
        {
            return  [ 
                    new KeyValuePair<string, object>(Constants.Application.PartitionKeyFieldName, request.DocType),
                    new KeyValuePair<string, object>(Constants.Application.DocTypeFieldName, request.DocType),
                    new KeyValuePair<string, object>(request.QueryFilter, request.Value)
                    ];
        }
    }
}
