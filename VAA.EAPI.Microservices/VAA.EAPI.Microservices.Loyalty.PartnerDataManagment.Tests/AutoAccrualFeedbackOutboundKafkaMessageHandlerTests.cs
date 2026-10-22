using System.Text.Json;
using System.Globalization;
using Microsoft.Azure.Cosmos.Table;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services;
using Xunit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class AutoAccrualFeedbackOutboundKafkaMessageHandlerTests
    {
        public AutoAccrualFeedbackOutboundKafkaMessageHandlerTests()
        {
            ConfigReader.Set(Constants.Environment.VsPgpPrivateKey, "test-private-key");
            ConfigReader.Set(Constants.Environment.MicroServiceDBKey, "test-storage-connection");
        }

        [Fact]
        public async Task HandleAsync_StoresEncryptedPayloadBeforeForwardingFeedback()
        {
            var tableStorage = new FakeAutoAccrualFeedbackTableStorage();
            var messageLookupTableStorage = new FakeAutoAccrualFeedbackMessageLookupTableStorage();
            var tableStorageInsertService = new FakeTableStorageInsertService();
            var payload = CreateFeedbackPayload("ui123");
            var feedbackHandler = new FakeFeedbackEventHandler { Result = true };
            var decryptedPayload = JsonSerializer.Serialize(payload);
            var kafkaMessageHandler = new AutoAccrualFeedbackOutboundKafkaMessageHandler(
                new FakePgpDecryptionService(decryptedPayload),
                tableStorage,
                messageLookupTableStorage,
                tableStorageInsertService,
                new FakePartnerDataService(),
                feedbackHandler);

            await kafkaMessageHandler.HandleAsync(CreateEnvelope(), CancellationToken.None);

            Assert.NotNull(tableStorage.Message);
            Assert.Equal("b5e94aa1-6c9c-4f89-94c2-4eb7ddf52a3e", tableStorage.Message!.PartitionKey);
            Assert.Equal("b39722bc-5eba-4e77-84c5-af3f2f0a276a", tableStorage.Message.RowKey);
            Assert.Equal("VN", tableStorage.Message.Producer);
            Assert.Equal("b5e94aa1-6c9c-4f89-94c2-4eb7ddf52a3e", tableStorage.Message.CorrelationId);
            Assert.Equal("ZW5jcnlwdGVkLXBheWxvYWQ=", tableStorage.Message.EncryptedPayload);
            Assert.Equal(Constants.Application.AutoAccrualFeedbackProcessedSuccessfullyStatus, tableStorage.Message.Status);
            Assert.Equal(string.Empty, tableStorage.Message.ErrorCode);
            Assert.False(tableStorage.Message.ParsedFeedbackRequest.IsNullOrWhiteSpace());
            Assert.Equal("ui123", feedbackHandler.Message?.UniqueIdentification?.UniqueIdentificationNumber);
            Assert.Equal("b39722bc-5eba-4e77-84c5-af3f2f0a276a", tableStorageInsertService.Message?.PartitionKey);
            Assert.Equal("b39722bc-5eba-4e77-84c5-af3f2f0a276a", tableStorageInsertService.Message?.RowKey);
            Assert.Equal("b5e94aa1-6c9c-4f89-94c2-4eb7ddf52a3e", tableStorageInsertService.Message?.CorrelationId);
        }

        [Fact]
        public async Task HandleAsync_StoresNlaErrorCode_WhenPayloadPostingStatusIsInvalid()
        {
            var tableStorage = new FakeAutoAccrualFeedbackTableStorage();
            var invalidPayload = CreateFeedbackPayload("ui123");
            invalidPayload.PostingDetails!.PostingStatus = "Invalid";
            var kafkaMessageHandler = new AutoAccrualFeedbackOutboundKafkaMessageHandler(
                new FakePgpDecryptionService(JsonSerializer.Serialize(invalidPayload)),
                tableStorage,
                new FakeAutoAccrualFeedbackMessageLookupTableStorage(),
                new FakeTableStorageInsertService(),
                new FakePartnerDataService(),
                new FakeFeedbackEventHandler { Result = true });

            await kafkaMessageHandler.HandleAsync(CreateEnvelope(), CancellationToken.None);

            Assert.Equal(Constants.Application.AutoAccrualFeedbackErrorStatus, tableStorage.Message?.Status);
            Assert.Equal(NlaErrorCodes.InvalidPostingStatus, tableStorage.Message?.ErrorCode);
        }

        [Fact]
        public async Task HandleAsync_PreservesSuccessfulOutcome_WhenMessageIdIsRepeated()
        {
            var tableStorage = new FakeAutoAccrualFeedbackTableStorage();
            var messageLookupTableStorage = new FakeAutoAccrualFeedbackMessageLookupTableStorage();
            var feedbackHandler = new FakeFeedbackEventHandler { Result = true };
            var kafkaMessageHandler = new AutoAccrualFeedbackOutboundKafkaMessageHandler(
                new FakePgpDecryptionService(JsonSerializer.Serialize(CreateFeedbackPayload("ui123"))),
                tableStorage,
                messageLookupTableStorage,
                new FakeTableStorageInsertService(),
                new FakePartnerDataService(),
                feedbackHandler);

            await kafkaMessageHandler.HandleAsync(CreateEnvelope(), CancellationToken.None);
            await kafkaMessageHandler.HandleAsync(CreateEnvelope(), CancellationToken.None);

            Assert.Equal(1, feedbackHandler.HandleCount);
            Assert.Equal(Constants.Application.AutoAccrualFeedbackProcessedSuccessfullyStatus, tableStorage.Message?.Status);
            Assert.Equal(string.Empty, tableStorage.Message?.ErrorCode);
        }

        [Fact]
        public async Task HandleAsync_StoresSchemaIdMismatch_WhenPayloadSchemaIdDiffersFromEnvelope()
        {
            var tableStorage = new FakeAutoAccrualFeedbackTableStorage();
            var payload = CreateFeedbackPayload("ui123");
            payload.SchemaId = "nla.feedback.abc";
            var kafkaMessageHandler = new AutoAccrualFeedbackOutboundKafkaMessageHandler(
                new FakePgpDecryptionService(JsonSerializer.Serialize(payload)),
                tableStorage,
                new FakeAutoAccrualFeedbackMessageLookupTableStorage(),
                new FakeTableStorageInsertService(),
                new FakePartnerDataService(),
                new FakeFeedbackEventHandler { Result = true });

            await kafkaMessageHandler.HandleAsync(CreateEnvelope(), CancellationToken.None);

            Assert.Equal(Constants.Application.AutoAccrualFeedbackErrorStatus, tableStorage.Message?.Status);
            Assert.Equal(NlaErrorCodes.EnvelopePayloadSchemaIdMismatch, tableStorage.Message?.ErrorCode);
        }

        private static string CreateEnvelope(
            string correlationId = "b5e94aa1-6c9c-4f89-94c2-4eb7ddf52a3e",
            string encryptedPayload = "ZW5jcnlwdGVkLXBheWxvYWQ=") => JsonSerializer.Serialize(new NlaEnvelopeBase
            {
                SchemaId = Constants.Application.FeedbackDbaSchemaId,
                SchemaVersion = Constants.Application.FeedbackDbaSchemaVersion,
                Producer = "VN",
                Consumer = Constants.Application.SkyTeamInboundConsumer,
                Timestamp = CreateUtcTimestamp(),
                MessageId = "b39722bc-5eba-4e77-84c5-af3f2f0a276a",
                CorrelationId = correlationId,
                EncryptionKeyId = "key-123",
                EncryptedPayload = encryptedPayload
            });

        private static string CreateUtcTimestamp() =>
            DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat, CultureInfo.InvariantCulture);

        private static DistanceBasedAccrualFeedbackPayload CreateFeedbackPayload(string uniqueIdentificationNumber) => new()
        {
            SchemaId = Constants.Application.FeedbackDbaSchemaId,
            SchemaVersion = Constants.Application.FeedbackDbaSchemaVersion,
            Producer = "VN",
            Consumer = Constants.Application.SkyTeamInboundConsumer,
            RequestType = Constants.Application.AutoAccrualRequestType,
            AccrualType = Constants.Application.DistanceBasedAccrualType,
            UniqueIdentification = new UniqueIdentification
            {
                UniqueIdentificationNumber = uniqueIdentificationNumber,
                AirlineCode = Constants.Application.SkyTeamInboundConsumer
            },
            PostingDetails = new PostingDetails
            {
                PostingStatus = Constants.Application.SuccessfulPostingStatus,
                PostingDateTime = CreateUtcTimestamp()
            },
            MileDetails = new MileDetails { BaseMiles = 1000 },
            ProductCode = "FLT"
        };

        private sealed class FakePgpDecryptionService(string decryptedPayload) : IPgpDecryptionService
        {
            public string? EncryptedPayload { get; private set; }

            public string Decrypt(string encryptedPayload, string privateKey)
            {
                EncryptedPayload = encryptedPayload;
                return decryptedPayload;
            }
        }

        private sealed class FakeFeedbackEventHandler : IAutoAccrualFeedbackOutboundEventHandler
        {
            public bool Result { get; set; }
            public DistanceBasedAccrualFeedbackPayload? Message { get; private set; }
            public int HandleCount { get; private set; }

            public Task<bool> Handle(DistanceBasedAccrualFeedbackPayload message)
            {
                HandleCount++;
                Message = message;
                return Task.FromResult(Result);
            }
        }

        private sealed class FakePartnerDataService : IPartnerDataService
        {
            public Task<IReadOnlyCollection<Partner>> GetSkyTeamPartnersAsync(CancellationToken cancellationToken) =>
                Task.FromResult<IReadOnlyCollection<Partner>>(
                [
                    new Partner { PartnerCode = "VN", SkyTeamPartnerFlag = true },
                    new Partner { PartnerCode = "VS", SkyTeamPartnerFlag = true }
                ]);
        }

        private sealed class FakeAutoAccrualFeedbackTableStorage : ITableStorageService<AutoAccrualFeedbackOutboundMessage>
        {
            public AutoAccrualFeedbackOutboundMessage? Message { get; private set; }
            public string TableName { get; set; } = string.Empty;
            public string ConnectionString { get; set; } = string.Empty;

            public Task<AutoAccrualFeedbackOutboundMessage> InsertOrMergeAsync(AutoAccrualFeedbackOutboundMessage entity)
            {
                Message = entity;
                return Task.FromResult(entity);
            }

            public Task<AutoAccrualFeedbackOutboundMessage> RetrieveAsync(string partitionKey, string rowKey) => throw new NotSupportedException();
            public Task<AutoAccrualFeedbackOutboundMessage> DeleteAsync(AutoAccrualFeedbackOutboundMessage entity) => throw new NotSupportedException();
            public Task<List<AutoAccrualFeedbackOutboundMessage>> RetrieveItemsAsync(TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<AutoAccrualFeedbackOutboundMessage>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<AutoAccrualFeedbackOutboundMessage>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken) => throw new NotSupportedException();
        }

        private sealed class FakeAutoAccrualFeedbackMessageLookupTableStorage : ITableStorageService<AutoAccrualFeedbackMessageLookup>
        {
            public AutoAccrualFeedbackMessageLookup? Message { get; private set; }
            public string TableName { get; set; } = string.Empty;
            public string ConnectionString { get; set; } = string.Empty;

            public Task<AutoAccrualFeedbackMessageLookup> InsertOrMergeAsync(AutoAccrualFeedbackMessageLookup entity)
            {
                Message = entity;
                return Task.FromResult(entity);
            }

            public Task<AutoAccrualFeedbackMessageLookup> RetrieveAsync(string partitionKey, string rowKey) => Task.FromResult(Message!);
            public Task<AutoAccrualFeedbackMessageLookup> DeleteAsync(AutoAccrualFeedbackMessageLookup entity) => throw new NotSupportedException();
            public Task<List<AutoAccrualFeedbackMessageLookup>> RetrieveItemsAsync(TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<AutoAccrualFeedbackMessageLookup>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<AutoAccrualFeedbackMessageLookup>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken) => throw new NotSupportedException();
        }

        private sealed class FakeTableStorageInsertService : ITableStorageInsertService
        {
            private readonly HashSet<string> messageIds = [];

            public AutoAccrualFeedbackMessageLookup? Message { get; private set; }

            public Task<bool> TryInsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken)
                where T : TableEntity
            {
                Message = entity as AutoAccrualFeedbackMessageLookup;
                return Task.FromResult(messageIds.Add($"{entity.PartitionKey}/{entity.RowKey}"));
            }
        }
    }
}
