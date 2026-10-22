using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class SkyTeamAutoAccrualInboundSubscriber
    {
        private readonly IAutoAccrualInboundHandler _handler;
        private readonly IAutoAccrualFeedbackPublisher _feedbackPublisher;
        private readonly ILogger<SkyTeamAutoAccrualInboundSubscriber> _logger;

        public SkyTeamAutoAccrualInboundSubscriber(
            IAutoAccrualInboundHandler handler,
            IAutoAccrualFeedbackPublisher feedbackPublisher,
            ILogger<SkyTeamAutoAccrualInboundSubscriber> logger)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _feedbackPublisher = feedbackPublisher ?? throw new ArgumentNullException(nameof(feedbackPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function(nameof(SkyTeamAutoAccrualInboundSubscriber))]
        public async Task ProcessMessageAsync(
            [ServiceBusTrigger(
                Constants.Application.ServiceBusTopicName,
                Constants.Application.AutoAccrualInboundSubscriptionName,
                Connection = "ServiceBusConnectionString")] string eventRequestPayload,
            CancellationToken cancellationToken)
        {
            var inboundEvent = Json.Deserialize<AutoAccrualInboundEvent>(eventRequestPayload)
                ?? throw new InvalidOperationException("Auto accrual inbound message deserialized to null.");
            AsyncContext.Current?.MapFromCloudEvent(inboundEvent);
            var correlationId = inboundEvent.Data?.CorrelationId ?? string.Empty;
            var messageId = inboundEvent.Data?.MessageId;

            if (string.IsNullOrWhiteSpace(messageId))
            {
                throw new InvalidOperationException("Auto accrual inbound message is missing a message ID.");
            }

            try
            {
                var transaction = await _handler.HandleAsync(eventRequestPayload, cancellationToken);
                await _feedbackPublisher.PublishSuccessAsync(correlationId, messageId, transaction, cancellationToken);
            }
            catch (AutoAccrualValidationException ex)
            {
                await _feedbackPublisher.PublishErrorAsync(correlationId, messageId, ex.ErrorCode, ex.Message, cancellationToken);
                _logger.LogError("Auto Accrual inbound message rejected. CorrelationId: {CorrelationId}. Reason: {Reason}", correlationId, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Auto Accrual inbound message.", eventRequestPayload);
                throw;
            }
        }

    }
}
