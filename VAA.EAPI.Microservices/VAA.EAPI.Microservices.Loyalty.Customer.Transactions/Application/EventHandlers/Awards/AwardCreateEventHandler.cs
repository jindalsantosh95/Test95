using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Awards
{
    public class AwardCreateEvent : TransactionBaseEvent, IRequest<Result<Unit>>
    {
        public AwardCreateEvent()
        {
            EventType = CosmosModificationType.Create;
        }
    }
    public class AwardCreateEventHandler : TransactionBaseEventHandler, IRequestHandler<AwardCreateEvent, Result<Unit>>
    {
        public AwardCreateEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Unit>> Handle(AwardCreateEvent request, CancellationToken cancellationToken)
        {
            var response = await base.Handle(request, cancellationToken);

            if (response.IsSuccess == true)
            {
                Award transactionEventData = Json.DeserializeObject<AwardCommandBase>((JToken)request.Data).Award;
                transactionEventData.Id = request.Id;

                CreateAwardAuditDoc(transactionEventData);
            }

            return response;
        }

        protected /*async*/ override Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken)
        {
            AwardCommandBase transactionEventData = Json.DeserializeObject<AwardCommandBase>((JToken)transactionEvent.Data);

            var validationResult = new AwardCreateValidator().Validate(transactionEventData.Award);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Award create request contains invalid data or missing mandatory fields.", validationResult.Errors);

            AwardDTO dto = new(transactionEventData.Award);

            dto.Id = transactionEvent.Id;
            dto.CreatedBy = transactionEventData.Award.CreatedBy ?? transactionEvent.Originator;
            dto.LastUpdatedBy = transactionEventData.Award.CreatedBy ?? transactionEvent.Originator;

            return Task.FromResult(new CosmosDocumentEvent(dto, CosmosModificationType.Create));
        }

        private void CreateAwardAuditDoc(Award award)
        {
            AwardAuditDTO auditDto = (AwardAuditDTO)award;
            auditDto.Id = Guid.NewGuid().ToString();
            auditDto.AwardName = award.AwardName;

            AwardAuditEvent awardAuditEvent = new AwardAuditEvent()
            {
                awardAudit = auditDto
            };

            var cloudEvent = AuditAwardEventToCloudEventMapper.CommandToEventMapper(awardAuditEvent);

            try
            {
                _mediator.Send(new EventRequestInsertCommand
                {
                    Id = cloudEvent.Id,
                    Type = cloudEvent.Type,
                    Data = cloudEvent,
                    Subject = cloudEvent.Subject
                });

                _mediator.Publish(cloudEvent);
            }
            catch
            {
                _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                throw;
            }
        }
    }
}
