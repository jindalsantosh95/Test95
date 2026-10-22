using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Awards
{
    public class TopMemberEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public TopMemberEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
    public class TopMemberEventHandler : TransactionBaseEventHandler, IRequestHandler<TopMemberEvent, Result<Unit>>
    {
        public TopMemberEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public Task<Result<Unit>> Handle(TopMemberEvent request, CancellationToken cancellationToken)
        {
            return base.Handle(request, cancellationToken);
        }

        protected async override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Validating Top Member award");

            // Extract the Award from the event
            AwardCommandBase topMemberEventData = Json.DeserializeObject<AwardCommandBase>((JToken)transactionEvent.Data);

            // Check if the award already exists
            Award currentTopMemberAward = (await AwardCommandBase.GetListOfAwards(_mediator, topMemberEventData.CustomerId, cancellationToken))
                                                .FirstOrDefault(a => a.ProductId == topMemberEventData.ProductId);

            // Doesn't exists, so create one
            if (currentTopMemberAward is null)
            {
                currentTopMemberAward = new()
                {
                    Id = transactionEvent.Id,
                    AwardId = transactionEvent.Id,
                    CreatedBy = transactionEvent.Originator,
                    CreatedOn = DateTime.UtcNow,
                    LastUpdatedBy = transactionEvent.Originator,
                    LastUpdatedOn = DateTime.UtcNow,
                    StartDate = DateTime.Today,
                    ProductId = topMemberEventData.ProductId,
                    CustomerId = topMemberEventData.CustomerId,
                    AwardValue = 0,
                };

                return new CosmosDocumentEvent(new AwardDTO(currentTopMemberAward), CosmosModificationType.Create);
            }
            else
            {
                // Reset the Start / Expiry
                currentTopMemberAward.LastUpdatedOn = DateTime.UtcNow;
                currentTopMemberAward.LastUpdatedBy = transactionEvent.Originator;
                currentTopMemberAward.StartDate = DateTime.UtcNow.Date;
                currentTopMemberAward.ExpiryDate = null;

                return new CosmosDocumentEvent(new AwardDTO(currentTopMemberAward), CosmosModificationType.Replace);
            }
        }
    }
}
