using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class SkyTeamAutoAccrualInboundSubscriberTests
    {
        [Fact]
        public async Task ProcessMessageAsync_WhenValidationSucceeds_PublishesSuccessFeedback()
        {
            var feedbackPublisher = new FakeFeedbackPublisher();
            var subscriber = new SkyTeamAutoAccrualInboundSubscriber(
                new SuccessfulHandler(),
                feedbackPublisher,
                NullLogger<SkyTeamAutoAccrualInboundSubscriber>.Instance);

            await subscriber.ProcessMessageAsync(Payload("correlation-id", "message-id"), CancellationToken.None);

            Assert.Equal("correlation-id", feedbackPublisher.CorrelationId);
            Assert.Equal("message-id", feedbackPublisher.MessageId);
            Assert.Equal("Success", feedbackPublisher.Status);
            Assert.Equal("transaction-id", feedbackPublisher.TransactionId);
        }

        [Fact]
        public async Task ProcessMessageAsync_WhenValidationFails_PublishesErrorFeedback()
        {
            var feedbackPublisher = new FakeFeedbackPublisher();
            var subscriber = new SkyTeamAutoAccrualInboundSubscriber(
                new ValidationFailureHandler(),
                feedbackPublisher,
                NullLogger<SkyTeamAutoAccrualInboundSubscriber>.Instance);

            await subscriber.ProcessMessageAsync(Payload("correlation-id", "message-id"), CancellationToken.None);

            Assert.Equal("correlation-id", feedbackPublisher.CorrelationId);
            Assert.Equal("message-id", feedbackPublisher.MessageId);
            Assert.Equal("Error", feedbackPublisher.Status);
            Assert.Equal("050.001", feedbackPublisher.ErrorCode);
            Assert.Equal("PNR name validation failed.", feedbackPublisher.ErrorDescription);
        }

        private static string Payload(string correlationId, string messageId) => JsonSerializer.Serialize(new
        {
            data = new
            {
                correlationId,
                messageId,
                accrualRequest = new { producer = "VN" }
            }
        });

        private sealed class SuccessfulHandler : IAutoAccrualInboundHandler
        {
            public Task<TransactionDocument> HandleAsync(string eventRequestPayload, CancellationToken cancellationToken) => Task.FromResult(new TransactionDocument { TransactionId = "transaction-id" });
        }

        private sealed class ValidationFailureHandler : IAutoAccrualInboundHandler
        {
            public Task<TransactionDocument> HandleAsync(string eventRequestPayload, CancellationToken cancellationToken) =>
                throw new AutoAccrualValidationException("PNR name validation failed.", "050.001");
        }

        private sealed class FakeFeedbackPublisher : IAutoAccrualFeedbackPublisher
        {
            public string CorrelationId { get; private set; } = string.Empty;
            public string MessageId { get; private set; } = string.Empty;
            public string Status { get; private set; } = string.Empty;
            public string TransactionId { get; private set; } = string.Empty;
            public string ErrorCode { get; private set; } = string.Empty;
            public string ErrorDescription { get; private set; } = string.Empty;

            public Task PublishSuccessAsync(string correlationId, string messageId, TransactionDocument transaction, CancellationToken cancellationToken)
            {
                CorrelationId = correlationId;
                MessageId = messageId;
                Status = "Success";
                TransactionId = transaction.TransactionId;
                return Task.CompletedTask;
            }

            public Task PublishErrorAsync(string correlationId, string messageId, string errorCode, string errorDescription, CancellationToken cancellationToken)
            {
                CorrelationId = correlationId;
                MessageId = messageId;
                Status = "Error";
                ErrorCode = errorCode;
                ErrorDescription = errorDescription;
                return Task.CompletedTask;
            }
        }
    }
}
