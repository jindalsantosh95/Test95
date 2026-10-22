using Microsoft.Extensions.Logging;
using System.Text;
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
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public class SkyTeamInboundAutoAccrualEventHandler : ISkyTeamInboundAutoAccrualEventHandler
    {
        private readonly ILogger<SkyTeamInboundAutoAccrualEventHandler> _logger;
        private readonly IPgpDecryptionService _pgpDecryptionService;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly ITableStorageService<SkyTeamInboundDataEntity> _skyTeamInboundDataTableStorage;
        private readonly ITableStorageService<SkyTeamNlaMessageLookupEntity> _skyTeamInboundMessageLookupTableStorage;
        private readonly ITableStorageInsertService _tableStorageInsertService;

        public SkyTeamInboundAutoAccrualEventHandler(ILogger<SkyTeamInboundAutoAccrualEventHandler> logger, IPgpDecryptionService pgpDecryptionService, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ITableStorageService<SkyTeamInboundDataEntity> skyTeamInboundDataTableStorage, ITableStorageService<SkyTeamNlaMessageLookupEntity> skyTeamInboundMessageLookupTableStorage, ITableStorageInsertService tableStorageInsertService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pgpDecryptionService = pgpDecryptionService ?? throw new ArgumentNullException(nameof(pgpDecryptionService));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _skyTeamInboundDataTableStorage = skyTeamInboundDataTableStorage ?? throw new ArgumentNullException(nameof(skyTeamInboundDataTableStorage));
            _skyTeamInboundMessageLookupTableStorage = skyTeamInboundMessageLookupTableStorage ?? throw new ArgumentNullException(nameof(skyTeamInboundMessageLookupTableStorage));
            _tableStorageInsertService = tableStorageInsertService ?? throw new ArgumentNullException(nameof(tableStorageInsertService));
            _skyTeamInboundDataTableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _skyTeamInboundDataTableStorage.TableName = ConfigReader.Get(Constants.Environment.SkyTeamInboundDataTableName);
            _skyTeamInboundMessageLookupTableStorage.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _skyTeamInboundMessageLookupTableStorage.TableName = ConfigReader.Get(Constants.Environment.SkyTeamInboundAccrualMessageLookupTableName);
        }

        public async Task<bool> Handle(string inboundMessage, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (inboundMessage.IsNullOrWhiteSpace())
                {
                    _logger.LogError("Inbound Kafka message is empty.");
                    return false;
                }

                NlaEnvelopeBase envelope;
                try
                {
                    envelope = Json.Deserialize<NlaEnvelopeBase>(inboundMessage);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Inbound auto-accrual envelope is not valid JSON. ErrorCode: {ErrorCode}", NlaErrorCodes.UnparseableEnvelope);
                    return false;
                }

                if (Encoding.UTF8.GetByteCount(inboundMessage) > 8192)
                {
                    return await StoreValidationFailureAsync(envelope, inboundMessage, NlaErrorCodes.EnvelopeTooLarge);
                }

                if (!NlaEnvelopeValidator.IsValidEnvelope(envelope, Constants.Application.AccrualDbaSchemaId, Constants.Application.AccrualDbaSchemaVersion, out var correlationId, out var validationErrorCode))
                {
                    return await StoreValidationFailureAsync(envelope, inboundMessage, validationErrorCode);
                }

                if (!string.Equals(envelope.Consumer, Constants.Application.SkyTeamInboundConsumer, StringComparison.Ordinal))
                {
                    return await StoreValidationFailureAsync(envelope, inboundMessage, NlaErrorCodes.InvalidEnvelopeConsumerFormat);
                }

                var privateKey = ConfigReader.Get(Constants.Environment.VsPgpPrivateKey);

                if (privateKey.IsNullOrWhiteSpace())
                {
                    _logger.LogError("SkyTeam PGP private key is missing.");
                    return false;
                }

                string decryptedPayload;
                try
                {
                    decryptedPayload = _pgpDecryptionService.Decrypt(envelope.EncryptedPayload, privateKey);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to decrypt SkyTeam inbound auto accrual payload.");
                    return await StoreValidationFailureAsync(envelope, inboundMessage, NlaErrorCodes.PgpDecryptionFailed);
                }

                if (decryptedPayload.IsNullOrWhiteSpace())
                {
                    return await StoreValidationFailureAsync(envelope, inboundMessage, NlaErrorCodes.InvalidDecryptedPayload);
                }

                return await StoreAndPublishAsync(envelope, inboundMessage, decryptedPayload, correlationId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SkyTeam inbound auto accrual message.");
                return false;
            }
        }

        private async Task<bool> StoreAndPublishAsync(NlaEnvelopeBase envelope, string inboundMessage, string decryptedPayload, Guid correlationId, CancellationToken cancellationToken)
        {
            CloudEvent? cloudEvent = null;
            SkyTeamNlaMessageLookupEntity? lookupEntity = null;
            var isLookupInserted = false;

            try
            {
                lookupEntity = new SkyTeamNlaMessageLookupEntity { PartitionKey = envelope.MessageId, RowKey = envelope.MessageId, CorrelationId = correlationId.ToString() };
                var isNewMessage = await _tableStorageInsertService.TryInsertAsync(ConfigReader.Get(Constants.Environment.SkyTeamInboundAccrualMessageLookupTableName), lookupEntity, CancellationToken.None);

                if (!isNewMessage)
                {
                    return true;
                }

                isLookupInserted = true;

                await _skyTeamInboundDataTableStorage.InsertOrMergeAsync(new SkyTeamInboundDataEntity { PartitionKey = correlationId.ToString(), RowKey = envelope.MessageId, Envelope = inboundMessage, DecryptedPayload = decryptedPayload, CorrelationId = correlationId.ToString(), Producer = envelope.Producer, NlaMessageCreatedAtUtc = envelope.Timestamp, ProcessingStatus = Constants.Application.ClaimStatus, ErrorCode = null });

                cloudEvent = CreateProcessingEvent(correlationId, envelope.MessageId);
                AsyncContext.Current?.MapToCloudEvent(cloudEvent);
                cloudEvent.CorrelationId = correlationId;

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.PdmEventTopicName }, cancellationToken);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await CleanupAsync(cloudEvent, lookupEntity, isLookupInserted);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing and publishing SkyTeam inbound auto accrual message.");
                await CleanupAsync(cloudEvent, lookupEntity, isLookupInserted);
                return false;
            }
        }

        private async Task CleanupAsync(CloudEvent? cloudEvent, SkyTeamNlaMessageLookupEntity? lookupEntity, bool isLookupInserted)
        {
            try
            {
                if (cloudEvent is not null)
                {
                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);
                }
            }
            finally
            {
                if (isLookupInserted)
                {
                    await _skyTeamInboundMessageLookupTableStorage.DeleteAsync(lookupEntity!);
                }
            }
        }

        private async Task<bool> StoreValidationFailureAsync(NlaEnvelopeBase? envelope, string inboundMessage, string validationErrorCode)
        {
            if (!NlaEnvelopeValidator.HasValidMessageId(envelope))
            {
                _logger.LogError("Inbound Kafka message failed NLA DBA envelope validation with error code {ErrorCode}", validationErrorCode);
                return false;
            }

            await _skyTeamInboundDataTableStorage.InsertOrMergeAsync(new SkyTeamInboundDataEntity { PartitionKey = GetPartitionKey(envelope!), RowKey = envelope.MessageId, Envelope = inboundMessage, CorrelationId = envelope.CorrelationId, Producer = envelope.Producer, NlaMessageCreatedAtUtc = envelope.Timestamp, ProcessingStatus = Constants.Application.ErrorStatus, ErrorCode = validationErrorCode });
            _logger.LogError("Inbound Kafka message failed NLA DBA envelope validation with error code {ErrorCode}", validationErrorCode);
            return true;
        }

        private static CloudEvent CreateProcessingEvent(Guid correlationId, string messageId)
        {
            return new CloudEvent
            {
                Id = messageId,
                CorrelationId = correlationId,
                Type = EnumOperations.GetEnumDescription(SkyTeamNlaEventType.InboundAutoAccrualProcessing),
                Source = Constants.Application.PdmEventTopicName,
                Subject = EnumOperations.GetEnumDescription(SkyTeamNlaEventType.InboundAutoAccrualProcessing),
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new AutoAccrualProcessingPayload { CorrelationId = correlationId, MessageId = messageId }
            };
        }

        private static string GetPartitionKey(NlaEnvelopeBase envelope) => Guid.TryParseExact(envelope.CorrelationId, "D", out _) ? envelope.CorrelationId : envelope.MessageId;
    }
}
