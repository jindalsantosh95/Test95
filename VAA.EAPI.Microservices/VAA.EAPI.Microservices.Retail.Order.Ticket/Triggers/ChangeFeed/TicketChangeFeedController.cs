using System;
using System.Collections.Generic;
using MediatR;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketData;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.TicketChangeFeed;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Triggers.ChangeFeed
{
    public class TicketChangeFeedController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TicketChangeFeedController> _logger;

        public TicketChangeFeedController(IMediator mediator, ILogger<TicketChangeFeedController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("TicketChangeFeed")]
        public async Task Run([CosmosDBTrigger(databaseName: "%TicketCosmosDBName%", containerName: Constants.Application.TicketCollection, Connection =  Constants.Application.CosmosDBConnectionString, LeaseContainerName = Constants.Application.LeaseCollectionName, CreateLeaseContainerIfNotExists = true, MaxItemsPerInvocation = Constants.Application.MaxItemsPerInvocation)]IReadOnlyList<TicketDto> documents)
        {
            foreach (var doc in documents)
            {
                if (doc != null && doc.DocType == DocumentType.TicketDocument.ToString())
                {
                    try
                    {
                        await _mediator.Send(new TicketDocFeedProcessCommand(doc));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error in handling ticket feed document while inserting to db docType {doc?.DocType}");
                    }
                }
            }
        }
    }
}
