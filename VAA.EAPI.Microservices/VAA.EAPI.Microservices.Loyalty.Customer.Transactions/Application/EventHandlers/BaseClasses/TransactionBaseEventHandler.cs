using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Repository.CustomerTransactions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.BaseClasses
{
    public class TransactionBaseEvent : CloudEvent
    {
        public CosmosModificationType EventType { get; set; }
    }

    public abstract class TransactionBaseEventHandler
    {
        protected readonly IMediator _mediator;
        protected readonly ILogger<TransactionBaseEventHandler> _logger;

        public TransactionBaseEventHandler(IMediator mediator, ILogger<TransactionBaseEventHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        protected abstract Task<CosmosDocumentEvent> ValidateEvent(TransactionBaseEvent transactionEvent, CancellationToken cancellationToken);

        public async Task<Result<Unit>> Handle(TransactionBaseEvent request, CancellationToken cancellationToken)
        {
            Result<Response<CosmosDocumentEvent>> result = null;

            for (int currentRetry = 0; currentRetry < Constants.Application.MaxNumberOfRetries; currentRetry++)
            {
                try
                {
                    _logger.LogDebug($"Validating event data ({request.Id})");
                    var validatedEvent = await ValidateEvent(request, cancellationToken);
                    _logger.LogDebug($"Validating successful ({request.Id})");


                    // Send to database
                    await _mediator.Send(new CustomerTransactionRepoCommand(validatedEvent), cancellationToken);

                    // Create response for Status database.
                    result = Result.Success(
                        new Response<CosmosDocumentEvent>()
                        {
                            Status = Status.OK,
                            Value = validatedEvent.ModificationType == CosmosModificationType.Delete ? null : validatedEvent
                        }
                        );
                    break;
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
                    break;
                }
                catch (Exception ex)
                {
                    result = Result.Failure<Response<CosmosDocumentEvent>>(ex.Message.ToString());
                    if (!ex.Message.Contains(Constants.Application.ConcurrentUpdateExceptionMessage1) || !ex.Message.Contains(Constants.Application.ConcurrentUpdateExceptionMessage2))
                    {
                        break;
                    }
                }
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
            
            if (ex.ValidationErrors.Any(x => x.Contains(ErrorCodes.DuplicateTransactionVocuherAccrual)))
            {
                response.FirstOrDefault().DeveloperMessage = ex.InnerException.Message;
                response.FirstOrDefault().Message = ex.InnerException.Message;
            }

            return await Task.FromResult(response);
        }

    }
}
