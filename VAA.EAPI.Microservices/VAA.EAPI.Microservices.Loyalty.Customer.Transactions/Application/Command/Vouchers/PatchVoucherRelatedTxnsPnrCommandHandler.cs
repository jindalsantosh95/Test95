using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.Vouchers
{
    public class PatchVoucherRelatedTxnsPnrCommandHandler : IPatchVoucherRelatedTxnsPnrCommandHandler
    {
        private readonly ILogger<PatchVoucherRelatedTxnsPnrCommandHandler> _logger;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;


        public PatchVoucherRelatedTxnsPnrCommandHandler(ILogger<PatchVoucherRelatedTxnsPnrCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        }

        public async Task<Result<Response<TransactionCommandResponse>>> Handle(PatchVoucherRelatedTxnsPnrCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            CloudEvent cloudEvent = null;

            try
            {
                cloudEvent = new CloudEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), request.EventType.ToString())),
                    Source = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName),
                    Originator = request.Originator,
                    Subject = EnumOperations.GetEnumDescription((TransactionCommandType)Enum.Parse(typeof(TransactionCommandType), request.EventType.ToString())),
                    Time = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                    Specversion = Constants.Application.SpecVersion,
                    Data = request.PatchVoucherRelatedTxnsPnrRequest
                };

                AsyncContext.Current.MapToCloudEvent(cloudEvent);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.EnvironmentSettings.ServiceBusTopicName) }, cancellationToken);

                return Result.Success(new Response<TransactionCommandResponse>
                {
                    Status = Status.Accepted,
                    HeaderParameters = new System.Collections.Generic.List<ResponseHeader>()
                        {
                            new()
                            {
                                Key = "location",
                                Value = string.Format(ConfigReader.Get(Constants.EnvironmentSettings.CommandStatusLocation).Replace("{0}s", "{0}"), Constants.Application.UpdateTransactionsPnr, cloudEvent.Id)
                            },
                            new()
                            {
                                Key = "CorrelationID",
                                Value = cloudEvent.CorrelationId.ToString()
                            }
                        }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in PatchVoucherRelatedTxnsPnrCommandHandler: {ex.Message}");

                if (cloudEvent != null)
                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                return Result.Failure<Response<TransactionCommandResponse>>(ex.Message.ToString());
            }
        }
    }
}
