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
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public class SplitPnrCommandHandler : ISplitPnrCommand
    {
        private readonly ILogger<SplitPnrCommandHandler> _logger;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public SplitPnrCommandHandler(ILogger<SplitPnrCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler)
        {
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(SplitPnrRequest splitPnrRequest, CancellationToken cancellationToken)
        {
            var cloudEventId = string.Empty;

            try
            {
                if (splitPnrRequest == null) throw new ArgumentNullException(nameof(splitPnrRequest));

                var cloudEvent = SplitPnrCommandToEventMapper.CommandToEventMapper(splitPnrRequest);

                cloudEventId = cloudEvent.Id;

                var trimmedData = SplitPnrCommandToEventMapper.TrimCloudEventData(splitPnrRequest);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = trimmedData, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicName) }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Split PNR event processing failed for {EventId}", cloudEvent.Id);

                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = OrderEventType.SplitPnrInitiate.ToString(), Data = splitPnrRequest, Subject = Constants.Application.SplitPnrEventSubject }, cancellationToken);
                    return Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }

                var splitPnrResponse = SplitPnrEventCommandResponseMapper.SplitPnrResponseMapper(cloudEvent.Id);
                return splitPnrResponse != null ? Result.Success(splitPnrResponse) : Result.Failure<Response<string>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Split PNR command handling failed for {EventId}", cloudEventId);
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, cloudEventId, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                return Result.Failure<Response<string>>(Json.Serialize(errorResponse));
            }
        }
    }
}
