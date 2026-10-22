using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Audit.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Exceptions;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Application.EventHandlers.BaseClasses
{
    public class AuditBaseEvent : CloudEvent
    {
        public CosmosModificationType EventType { get; set; }
    }

    public abstract class AuditBaseEventHandler
    {
        protected readonly IMediator _mediator;

        public AuditBaseEventHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }


        protected abstract Task<CosmosDocumentEvent> ValidateEvent(AuditBaseEvent auditEvent, CancellationToken cancellationToken);

        public async Task<Result<Unit>> Handle(AuditBaseEvent request, CancellationToken cancellationToken)
        {
            Result<Response<CosmosDocumentEvent>> result = null;

            try
            {
                var validatedEvent = await ValidateEvent(request, cancellationToken);

                // Send to database
                await _mediator.Send(new AuditRepoCommand(validatedEvent), cancellationToken);

                // Create response for Status database.
                result = Result.Success(
                    new Response<CosmosDocumentEvent>()
                    {
                        Status = Status.OK,
                        Value = validatedEvent.ModificationType == CosmosModificationType.Delete ? null : validatedEvent
                    }
                    );
            }
            catch (RequestValidationException rvex)
            {
                result = Result.Success(
                    new Response<CosmosDocumentEvent>()
                    {
                        Status = Status.BusinessException,
                        Errors = (await MapValidationErrors(rvex)).ToList()
                    }
                    );
            }
            catch (Exception ex)
            {
                result = Result.Failure<Response<CosmosDocumentEvent>>(ex.Message.ToString());
            }

            // Send outcome to Status Database
            await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.CommandResponseStatusKey });

            return Unit.Value;
        }

        public static async Task<IEnumerable<ErrorDetails>> MapValidationErrors(RequestValidationException ex)
        {
            IEnumerable<ErrorDetails> response = new List<ErrorDetails>();

            foreach (var item in ex.ValidationErrors)
            {
                response = response.Concat(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(item), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath));
            }

            return await Task.FromResult(response);
        }

    }
}
