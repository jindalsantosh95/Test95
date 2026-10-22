using Microsoft.Extensions.Logging;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Validations;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public class SkyTeamInboundAutoAccrualProcessingEventHandler : ISkyTeamInboundAutoAccrualProcessingEventHandler
    {
        private readonly ILogger<SkyTeamInboundAutoAccrualProcessingEventHandler> _logger;
        private readonly ITableStorageService<SkyTeamInboundDataEntity> _skyTeamInboundDataTableStorage;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public SkyTeamInboundAutoAccrualProcessingEventHandler(ILogger<SkyTeamInboundAutoAccrualProcessingEventHandler> logger, ITableStorageService<SkyTeamInboundDataEntity> skyTeamInboundDataTableStorage, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _skyTeamInboundDataTableStorage = skyTeamInboundDataTableStorage ?? throw new ArgumentNullException(nameof(skyTeamInboundDataTableStorage));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _skyTeamInboundDataTableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _skyTeamInboundDataTableStorage.TableName = ConfigReader.Get(Constants.Environment.SkyTeamInboundDataTableName);
        }

        public async Task<bool> Handle(AutoAccrualProcessingPayload processingPayload, CancellationToken cancellationToken)
        {
            CloudEvent? loyaltyManagementEvent = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (processingPayload is null || processingPayload.CorrelationId == Guid.Empty || processingPayload.MessageId.IsNullOrWhiteSpace())
                {
                    _logger.LogError("SkyTeam auto accrual processing message is missing a correlation ID or message ID.");
                    return false;
                }

                var correlationId = processingPayload.CorrelationId;
                var storedData = await _skyTeamInboundDataTableStorage.RetrieveAsync(correlationId.ToString(), processingPayload.MessageId);

                if (storedData is null || storedData.DecryptedPayload.IsNullOrWhiteSpace())
                {
                    _logger.LogError("SkyTeam auto accrual decrypted data was not found for message ID {MessageId}.", processingPayload.MessageId);
                    return false;
                }

                if (storedData.ProcessingStatus == Constants.Application.ErrorStatus)
                {
                    return true;
                }

                DistanceBasedAccrualPayload? accrualRequest;
                try
                {
                    accrualRequest = Json.Deserialize<DistanceBasedAccrualPayload>(storedData.DecryptedPayload);
                }
                catch (Exception ex)
                {
                    return await StoreValidationErrorAsync(storedData, NlaErrorCodes.InvalidDecryptedPayload, ex);
                }

                if (accrualRequest is null)
                {
                    return await StoreValidationErrorAsync(storedData, NlaErrorCodes.InvalidDecryptedPayload);
                }

                NlaEnvelopeBase? envelope;
                try
                {
                    envelope = Json.Deserialize<NlaEnvelopeBase>(storedData.Envelope);
                }
                catch (Exception ex)
                {
                    return await StoreValidationErrorAsync(storedData, NlaErrorCodes.UnparseableEnvelope, ex);
                }

                var envelopePayloadErrorCode = GetEnvelopePayloadErrorCode(envelope, accrualRequest);

                if (envelopePayloadErrorCode is not null)
                {
                    return await StoreValidationErrorAsync(storedData, envelopePayloadErrorCode);
                }

                var validationResult = new SkyTeamInboundAutoAccrualRequestValidator().Validate(accrualRequest);

                if (!validationResult.IsValid)
                {
                    return await StoreValidationErrorAsync(storedData, validationResult.Errors[0].ErrorCode);
                }

                storedData.ParsedAccrualRequest = Json.Serialize(accrualRequest);
                await _skyTeamInboundDataTableStorage.InsertOrMergeAsync(storedData);

                loyaltyManagementEvent = CreateLoyaltyManagementEvent(correlationId, processingPayload.MessageId, accrualRequest);
                AsyncContext.Current?.MapToCloudEvent(loyaltyManagementEvent);
                loyaltyManagementEvent.CorrelationId = correlationId;

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = loyaltyManagementEvent.Id, Type = loyaltyManagementEvent.Type, Data = loyaltyManagementEvent.Data, Subject = loyaltyManagementEvent.Subject }, cancellationToken);
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = loyaltyManagementEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.LoyaltyMemberManagementTopicName) }, cancellationToken);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing SkyTeam auto accrual requests to Loyalty Management.");

                if (loyaltyManagementEvent is not null)
                {
                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = loyaltyManagementEvent.Id, Type = loyaltyManagementEvent.Type, Data = loyaltyManagementEvent.Data, Subject = loyaltyManagementEvent.Subject }, cancellationToken);
                }

                return false;
            }
        }

        private async Task<bool> StoreValidationErrorAsync(SkyTeamInboundDataEntity storedData, string errorCode, Exception? exception = null)
        {
            storedData.ProcessingStatus = Constants.Application.ErrorStatus;
            storedData.ErrorCode = errorCode;
            await _skyTeamInboundDataTableStorage.InsertOrMergeAsync(storedData);

            if (exception is null)
            {
                _logger.LogError("SkyTeam auto accrual request has missing or invalid mandatory fields for message ID {MessageId} with error code {ErrorCode}", storedData.RowKey, errorCode);
            }
            else
            {
                _logger.LogError(exception, "SkyTeam auto accrual payload could not be parsed for message ID {MessageId}.", storedData.RowKey);
            }

            return true;
        }

        private static string? GetEnvelopePayloadErrorCode(NlaEnvelopeBase? envelope, DistanceBasedAccrualPayload accrualRequest)
        {
            if (envelope is null) return NlaErrorCodes.UnparseableEnvelope;
            if (!string.Equals(envelope.Producer, accrualRequest.Producer, StringComparison.Ordinal)) return NlaErrorCodes.EnvelopePayloadProducerMismatch;
            if (!string.Equals(envelope.Consumer, accrualRequest.Consumer, StringComparison.Ordinal)) return NlaErrorCodes.EnvelopePayloadConsumerMismatch;
            if (!string.Equals(envelope.SchemaId, accrualRequest.SchemaId, StringComparison.Ordinal)) return NlaErrorCodes.EnvelopePayloadSchemaIdMismatch;
            return envelope.SchemaVersion == accrualRequest.SchemaVersion ? null : NlaErrorCodes.EnvelopePayloadSchemaVersionMismatch;
        }

        private static CloudEvent CreateLoyaltyManagementEvent(Guid correlationId, string messageId, DistanceBasedAccrualPayload accrualRequest)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                CorrelationId = correlationId,
                Type = EnumOperations.GetEnumDescription(SkyTeamNlaEventType.LoyaltyManagementRequest),
                Source = Constants.Application.PdmEventTopicName,
                Subject = EnumOperations.GetEnumDescription(SkyTeamNlaEventType.LoyaltyManagementRequest),
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new AutoAccrualLoyaltyManagementRequest { CorrelationId = correlationId, MessageId = messageId, AccrualRequest = accrualRequest }
            };
        }

    }
}
