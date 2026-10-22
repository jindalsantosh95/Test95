using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public class SkyTeamInboundFeedbackProcessingEventHandler : ISkyTeamInboundFeedbackProcessingEventHandler
    {
        private readonly ILogger<SkyTeamInboundFeedbackProcessingEventHandler> _logger;
        private readonly ITableStorageService<SkyTeamInboundDataEntity> _eventStore;
        private readonly IAutoAccrualFeedbackGenerationPublisher _feedbackGenerationPublisher;

        public SkyTeamInboundFeedbackProcessingEventHandler(
            ILogger<SkyTeamInboundFeedbackProcessingEventHandler> logger,
            ITableStorageService<SkyTeamInboundDataEntity> eventStore,
            IAutoAccrualFeedbackGenerationPublisher feedbackGenerationPublisher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
            _feedbackGenerationPublisher = feedbackGenerationPublisher ?? throw new ArgumentNullException(nameof(feedbackGenerationPublisher));
            _eventStore.ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey);
            _eventStore.TableName = ConfigReader.Get(Constants.Environment.SkyTeamInboundDataTableName);
        }

        public async Task<bool> Handle(AutoAccrualFeedbackProcessingPayload message, CancellationToken cancellationToken)
        {
            if (!IsValid(message, out var validationError))
            {
                _logger.LogError("Auto accrual feedback processing message is invalid: {ValidationError}", validationError);
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();
            var eventRecord = await _eventStore.RetrieveAsync(message.CorrelationId, message.MessageId);
            if (eventRecord is null)
            {
                _logger.LogError("PDM event-store record was not found for correlation ID {CorrelationId}", message.CorrelationId);
                return false;
            }

            if (eventRecord.ProcessingStatus.Is(Constants.Application.AutoAccrualFeedbackSentStatus))
            {
                _logger.LogError("Auto accrual feedback was already sent for correlation ID {CorrelationId} and message ID {MessageId}.", message.CorrelationId, message.MessageId);
                return true;
            }

            if (message.ProcessedStatus.Is(Constants.Application.AutoAccrualFeedbackErrorStatus) &&
                !IsMappedNlaErrorCode(message.ErrorDetails!.ErrorCode!, message.CorrelationId))
            {
                _logger.LogError(
                    "Auto accrual feedback error code {ErrorCode} is not mapped in the NLA error catalogue for correlation ID {CorrelationId} and message ID {MessageId}.",
                    message.ErrorDetails.ErrorCode,
                    message.CorrelationId,
                    message.MessageId);
                return false;
            }

            eventRecord.ProcessingStatus = message.ProcessedStatus;
            eventRecord.CorrelationId = message.CorrelationId;
            eventRecord.TransactionId = message.TransactionId;
            eventRecord.ErrorCode = message.ErrorDetails?.ErrorCode;
            eventRecord.FeedbackMileDetails = new SkyTeamInboundDataMileDetails
            {
                BaseMiles = message.MileDetails!.BaseMiles,
                ClassOfServiceBonusMiles = message.MileDetails.ClassOfServiceBonusMiles,
                EliteTierBonusMiles = message.MileDetails.EliteTierBonusMiles,
                PromotionalBonusMiles = message.MileDetails.PromotionalBonusMiles
            };
            await _eventStore.InsertOrMergeAsync(eventRecord);
            await _feedbackGenerationPublisher.PublishAsync(message.CorrelationId, message.MessageId, cancellationToken);
            return true;
        }

        private static bool IsValid(AutoAccrualFeedbackProcessingPayload? message, out string validationError)
        {
            validationError = string.Empty;
            if (message is null) validationError = "Message is required.";
            else if (message.CorrelationId.IsNullOrWhiteSpace()) validationError = "Correlation ID is required.";
            else if (message.MessageId.IsNullOrWhiteSpace()) validationError = "Message ID is required.";
            else if (!message.ProcessedStatus.Is("Success") && !message.ProcessedStatus.Is("Error")) validationError = "Processed status must be Success or Error.";
            else if (message.MileDetails?.BaseMiles is null) validationError = "Base miles are required.";
            else if (message.ProcessedStatus.Is("Error") && message.ErrorDetails?.ErrorCode.IsNullOrWhiteSpace() != false) validationError = "Error code is required when processed status is Error.";
            return validationError.Length == 0;
        }

        private static bool IsMappedNlaErrorCode(string errorCode, string correlationId)
        {
            var mappedError = ErrorMapper.MapBusinessExceptionToEAPIException(
                    errorCode,
                    correlationId,
                    "ErrorCodes/ErrorCodes_SkyTeam.json")
                .SingleOrDefault();

            return mappedError?.Code == errorCode;
        }
    }
}
