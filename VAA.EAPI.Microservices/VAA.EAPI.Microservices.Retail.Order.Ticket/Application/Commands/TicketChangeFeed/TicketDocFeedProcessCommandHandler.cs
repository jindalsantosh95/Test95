using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository.ChangeFeed;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.TicketChangeFeed
{
    public class TicketDocFeedProcessCommandHandler : IRequestHandler<TicketDocFeedProcessCommand, bool>
    {
        private readonly ILogger<TicketDocFeedProcessCommandHandler> _logger;
        private readonly IMediator _mediator;

        public TicketDocFeedProcessCommandHandler(IMediator mediator, ILogger<TicketDocFeedProcessCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(TicketDocFeedProcessCommand request, CancellationToken cancellationToken)
        {           
            var existingDoc = await _mediator.Send(new TicketFeedRepoQuery(value: request.TicketFeedDocument.Document.TktDocNb, docType: DocumentType.TicketFFPAndEmail.ToString(),"TicketNumber"));            

            if (existingDoc != null && existingDoc.Any())
            {                
                await _mediator.Send(new TicketFeedRepoCommand(new EAPI.Infrastructure.Repository.CosmosSqlDBv2.CosmosDocumentEvent((TicketFFPAndEmailDto)request.TicketFeedDocument, EAPI.Infrastructure.Entities.Enums.CosmosModificationType.Replace)));
            }
            else
            {
                await _mediator.Send(new TicketFeedRepoCommand(new EAPI.Infrastructure.Repository.CosmosSqlDBv2.CosmosDocumentEvent((TicketFFPAndEmailDto)request.TicketFeedDocument, EAPI.Infrastructure.Entities.Enums.CosmosModificationType.Create)));
            }

            return true;
        }
    }
}
