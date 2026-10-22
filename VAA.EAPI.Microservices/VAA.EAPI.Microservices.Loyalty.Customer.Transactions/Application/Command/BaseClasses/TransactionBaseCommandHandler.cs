using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.BaseClasses
{
    public abstract class TransactionBaseCommandHandler<T> where T : ICustomerTransactionCommandResponse
    {
        protected readonly IMediator _mediator;
        protected readonly ILogger<TransactionBaseCommandHandler<T>> _logger;

        public TransactionBaseCommandHandler(IMediator mediator, ILogger<TransactionBaseCommandHandler<T>> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<T>>> Handle(TransactionBaseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Create the event
                var cloudEvent = new CloudEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), request.EventType.ToString())),
                    Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                    Originator = request.Originator,
                    Subject = request.Subject,
                    Time = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    Specversion = Constants.Application.SpecVersion,
                    Data = request
                };

                // Record in Event Store
                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    // Publish to service bus
                    await _mediator.Publish(cloudEvent, cancellationToken);

                    // Send 202 ACCEPTED response
                    return Result.Success(new Response<T>
                    {
                        Status = Status.Accepted,
                        HeaderParameters = new System.Collections.Generic.List<ResponseHeader>()
                        {
                            new()
                            {
                                Key = "location",
                                Value = string.Format(ConfigReader.Get(Constants.EnvironmentSettings.CommandStatusLocation), request.DocType, cloudEvent.Id)
                            },
                            new()
                            {
                                Key = "CorrelationID",
                                Value = cloudEvent.Id
                            }
                        }
                    });
                }
                catch
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transaction Command Handler({typeof(T).GetType().Name}) - Request process failed\n", ex.StackTrace);
                return Result.Failure<Response<T>>(ex.Message.ToString());
            }
        }

        internal Response<T> GetCustomerErrorMessage(string errorCode, Status errorStatus)
        {
            return new Response<T>()
            {
                Status = errorStatus,
                Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
            };
        }
    }
}
