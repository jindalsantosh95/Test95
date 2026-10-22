using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public class SkyTeamInboundFeedbackGenerationEventHandler : ISkyTeamInboundFeedbackGenerationEventHandler
    {
        private readonly ILogger<SkyTeamInboundFeedbackGenerationEventHandler> _logger;
        private readonly ITableStorageService<SkyTeamInboundDataEntity> _eventStore;
        private readonly ISkyTeamAutoAccrualFeedbackKafkaPublisher _kafkaPublisher;
        private readonly ITableStorageInsertService _tableStorageInsertService;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreDeleteHandler;

        public SkyTeamInboundFeedbackGenerationEventHandler(
            ILogger<SkyTeamInboundFeedbackGenerationEventHandler> logger,
            ITableStorageService<SkyTeamInboundDataEntity> eventStore,
            ISkyTeamAutoAccrualFeedbackKafkaPublisher kafkaPublisher,
            ITableStorageInsertService tableStorageInsertService,
            EventStoreRequestDeleteCommandHandler eventStoreDeleteHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _kafkaPublisher = kafkaPublisher ?? throw new ArgumentNullException(nameof(kafkaPublisher));
            _tableStorageInsertService = tableStorageInsertService ?? throw new ArgumentNullException(nameof(tableStorageInsertService));
            _eventStoreDeleteHandler = eventStoreDeleteHandler ?? throw new ArgumentNullException(nameof(eventStoreDeleteHandler));
            _eventStore.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _eventStore.TableName = ConfigReader.Get(Constants.Environment.SkyTeamInboundDataTableName);
        }

        public async Task<bool> Handle(AutoAccrualFeedbackGenerationPayload message, CancellationToken cancellationToken)
        {
            if (message is null || message.CorrelationId.IsNullOrWhiteSpace() || message.MessageId.IsNullOrWhiteSpace())
            {
                _logger.LogError("Auto accrual feedback generation message is missing a correlation ID or message ID.");
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var eventRecord = await _eventStore.RetrieveAsync(message.CorrelationId, message.MessageId);
            if (eventRecord == null )
            {
                _logger.LogError("PDM event-store record was not found for correlation ID {CorrelationId}", message.CorrelationId);
                return false;
            }

            if (eventRecord.ProcessingStatus.Is(Constants.Application.AutoAccrualFeedbackSentStatus))
            {
                _logger.LogError("Auto accrual feedback was already sent for correlation ID {CorrelationId} and message ID {MessageId}.", message.CorrelationId, message.MessageId);
                return true;
            }

            var feedbackPublished = false;
            
                cancellationToken.ThrowIfCancellationRequested();
                var processing = GetProcessingOutcome(eventRecord, message.CorrelationId);
                var accrualRequest = GetAccrualRequest(eventRecord);
if (!HasProcessingOutcome(processing) || accrualRequest is null)
                {
                    _logger.LogError("PDM event-store record does not contain the accrual request and processing outcome for correlation ID {CorrelationId} and message ID {MessageId}.", message.CorrelationId, message.MessageId);
                    return false;
                }

                var feedback = MapFeedback(accrualRequest, processing);
                var feedbackEvent = new EventRequestInsertCommand
                {
                    Id = message.MessageId,
                    CorrelationId = Guid.TryParse(message.CorrelationId, out var correlationId) ? correlationId : Guid.Empty,
                    Type = Constants.Application.AutoAccrualFeedbackOutboundType,
                    Source = ConfigReader.Get(Constants.Environment.AutoAccrualFeedbackOutboundKafkaTopic),
                    Subject = Constants.Application.AutoAccrualFeedbackOutboundType,
                    Data = feedback
                };
                var isNewFeedbackEvent = await _tableStorageInsertService.TryInsertAsync(
                    ConfigReader.Get(Constants.Environment.MicroServiceEventStoreTableName),
                    new EventStoreDatabaseEntity
                    {
                        ID = feedbackEvent.Id,
                        PayLoad = Json.Serialize(feedbackEvent),
                        PartitionKey = feedbackEvent.Type,
                        RowKey = feedbackEvent.Id
                    },
                    cancellationToken);

                if (!isNewFeedbackEvent)
                {
                    _logger.LogInformation("Auto accrual feedback event has already been claimed for correlation ID {CorrelationId} and message ID {MessageId}.", message.CorrelationId, message.MessageId);
                    return true;
                }

                try
                {
                    await _kafkaPublisher.PublishAsync(feedback, message.CorrelationId, message.MessageId, cancellationToken);
                }
                catch
                {
                    await _eventStoreDeleteHandler.Handle(new EventStoreRequestDeleteCommand
                    {
                        Id = feedbackEvent.Id,
                        Type = feedbackEvent.Type
                    }, CancellationToken.None);
                    throw;
                }
                eventRecord.ProcessingStatus = Constants.Application.AutoAccrualFeedbackSentStatus;
                await _eventStore.InsertOrMergeAsync(eventRecord);
                feedbackPublished = true;
            

            if (!feedbackPublished)
            {
                _logger.LogError("No completed auto accrual records require feedback for correlation ID {CorrelationId}.", message.CorrelationId);
            }

            return true;
        }

        private static AutoAccrualFeedbackProcessingPayload GetProcessingOutcome(SkyTeamInboundDataEntity eventRecord, string correlationId) => new()
        {
            CorrelationId = correlationId,
            MessageId = eventRecord.RowKey,
            ProcessedStatus = eventRecord.ProcessingStatus,
            TransactionId = eventRecord.TransactionId ?? string.Empty,
            ErrorDetails = eventRecord.ErrorCode.IsNullOrWhiteSpace() ? null : new AutoAccrualFeedbackProcessingErrorDetails
            {
                ErrorCode = eventRecord.ErrorCode
            },
            MileDetails = eventRecord.FeedbackMileDetails is null ? null : new MileDetails
            {
                BaseMiles = eventRecord.FeedbackMileDetails.BaseMiles,
                ClassOfServiceBonusMiles = eventRecord.FeedbackMileDetails.ClassOfServiceBonusMiles,
                EliteTierBonusMiles = eventRecord.FeedbackMileDetails.EliteTierBonusMiles,
                PromotionalBonusMiles = eventRecord.FeedbackMileDetails.PromotionalBonusMiles
            }
        };

        private static bool HasProcessingOutcome(AutoAccrualFeedbackProcessingPayload processing) =>
            (processing.ProcessedStatus.Is("Success") || processing.ProcessedStatus.Is("Error")) &&
            processing.MileDetails?.BaseMiles is not null &&
            (!processing.ProcessedStatus.Is("Error") || processing.ErrorDetails?.ErrorCode.IsNullOrWhiteSpace() == false);

        private static DistanceBasedAccrualPayload? GetAccrualRequest(SkyTeamInboundDataEntity eventRecord)
        {
            var accrualRequest = eventRecord.ParsedAccrualRequest.IsNullOrWhiteSpace()
                ? eventRecord.DecryptedPayload
                : eventRecord.ParsedAccrualRequest;

            return accrualRequest.IsNullOrWhiteSpace()
                ? null
                : Json.Deserialize<DistanceBasedAccrualPayload>(accrualRequest);
        }

        private static DistanceBasedAccrualFeedbackPayload MapFeedback(
            DistanceBasedAccrualPayload accrualRequest,
            AutoAccrualFeedbackProcessingPayload processing)
        {
            var success = processing.ProcessedStatus.Is("Success");
            return new DistanceBasedAccrualFeedbackPayload
            {
                SchemaId = Constants.Application.FeedbackDbaSchemaId,
                SchemaVersion = Constants.Application.FeedbackDbaSchemaVersion,
                Producer = accrualRequest.Consumer,
                Consumer = accrualRequest.Producer,
                RequestType = accrualRequest.RequestType,
                AccrualType = accrualRequest.AccrualType,
                UniqueIdentification = accrualRequest.UniqueIdentification,
                PostingDetails = new PostingDetails
                {
                    PostingDateTime = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                    PostingStatus = success ? Constants.Application.SuccessfulPostingStatus : Constants.Application.UnsuccessfulPostingStatus
                },
                MileDetails = processing.MileDetails,
                ErrorInformation = success ? null : new BusinessErrorInformation
                {
                    ErrorDetails = [new BusinessErrorDetail
                    {
                        ErrorCode = processing.ErrorDetails!.ErrorCode!
                    }]
                },
                ProductCode = accrualRequest.ProductCode,
                AdditionalInformation = accrualRequest.AdditionalInformation
            };
        }
    }
}
