using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.IFG
{
    public class CreatePaymentDocumentCommandHandler : ICreatePaymentDocumentCommand
    {
        private readonly ILogger<CreatePaymentDocumentCommandHandler> _logger;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public CreatePaymentDocumentCommandHandler(ILogger<CreatePaymentDocumentCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler)
        {
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(PaymentDocRequest paymentDocRequest, CancellationToken cancellationToken)
        {
            var cloudEventId = Guid.NewGuid().ToString();

            try
            {
                if (paymentDocRequest == null) throw new ArgumentNullException(nameof(paymentDocRequest));

                var cloudEvent = CreatePaymentDocCommandToEventMapper.CommandToEventMapper(paymentDocRequest, cloudEventId);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.CreatePaymentDocumentTopicName }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Rollback event from Event store due to Error while publishing in CreatePaymentDocument Command Handler: {ex.Message}");

                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = Constants.Application.CreatePaymentDocumentEventType, Data = paymentDocRequest, Subject = Constants.Application.CreatePaymentDocumentEventSubject }, cancellationToken);
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }

                var createPaymentDocResponse = CreatePaymentDocCommandResponseMapper.CreatePaymentDocumentResponseMapper(cloudEvent.Id);
                return createPaymentDocResponse != null ? Result.Success(createPaymentDocResponse) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, cloudEventId, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}
