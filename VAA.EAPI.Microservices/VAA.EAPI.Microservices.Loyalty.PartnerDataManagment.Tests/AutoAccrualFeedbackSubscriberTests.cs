using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class AutoAccrualFeedbackSubscriberTests
    {
        [Theory]
        [InlineData("Success", "Y", null)]
        [InlineData("Error", "N", NlaErrorCodes.InvalidAirlineCode)]
        [InlineData("Error", "N", NlaErrorCodes.InvalidSurname)]
        [InlineData("Error", "N", NlaErrorCodes.InvalidGivenName)]
        [InlineData("Error", "N", NlaErrorCodes.FfpAccountNotFound)]
        [InlineData("Error", "N", NlaErrorCodes.FfpAccountSuspended)]
        public async Task Handle_StoresOutcomeAndPublishesMappedFeedback(
            string processedStatus,
            string postingStatus,
            string? errorCode)
        {
            ConfigReader.Set(Constants.Environment.MicroServiceDBKey, "test-storage");
            ConfigReader.Set(Constants.Environment.SkyTeamInboundDataTableName, "SkyTeamInboundAccrualData");
            ConfigReader.Set(Constants.Environment.AutoAccrualFeedbackOutboundKafkaTopic, "feedback-topic");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceEventStoreTableName, "EventStore");
            var correlationId = Guid.NewGuid().ToString();
            var messageId = "message-123";
            var tableStorage = new FakeTableStorage(
                CreateEventStoreRecord(correlationId, messageId),
                CreateEventStoreRecord(correlationId, "message-456"));
            var feedbackGenerationPublisher = new FakeFeedbackGenerationPublisher();
            var processingHandler = new SkyTeamInboundFeedbackProcessingEventHandler(NullLogger<SkyTeamInboundFeedbackProcessingEventHandler>.Instance, tableStorage, feedbackGenerationPublisher);
            var kafkaPublisher = new FakeSkyTeamAutoAccrualFeedbackKafkaPublisher();
            var feedbackEventStore = new FakeFeedbackEventStore();
            var generationHandler = new SkyTeamInboundFeedbackGenerationEventHandler(
                NullLogger<SkyTeamInboundFeedbackGenerationEventHandler>.Instance,
                tableStorage,
                kafkaPublisher,
                feedbackEventStore,
                new EventStoreRequestDeleteCommandHandler(feedbackEventStore, NullLogger<EventStoreRequestDeleteCommandHandler>.Instance));

            var outcome = new AutoAccrualFeedbackProcessingPayload
            {
                CorrelationId = correlationId,
                MessageId = messageId,
                ProcessedStatus = processedStatus,
                TransactionId = "transaction-123",
                MileDetails = new MileDetails { BaseMiles = 2500, ClassOfServiceBonusMiles = 500, EliteTierBonusMiles = 250, PromotionalBonusMiles = 0 },
                ErrorDetails = errorCode is not null
                    ? new AutoAccrualFeedbackProcessingErrorDetails
                    {
                        ErrorCode = errorCode,
                        ErrorDescription = "This description must be replaced by the catalogue mapping."
                    }
                    : null
            };

            Assert.True(await processingHandler.Handle(outcome, CancellationToken.None));
            var processedRecord = tableStorage.Get(correlationId, messageId);
            var unprocessedRecord = tableStorage.Get(correlationId, "message-456");
            Assert.Equal("transaction-123", processedRecord.TransactionId);
            Assert.Equal(processedStatus, processedRecord.ProcessingStatus);
            Assert.Equal(2500, processedRecord.FeedbackMileDetails!.BaseMiles);
            Assert.Equal(errorCode, processedRecord.ErrorCode);
            Assert.Empty(unprocessedRecord.ProcessingStatus);
            Assert.Equal([(correlationId, messageId)], feedbackGenerationPublisher.Messages);
            Assert.True(await generationHandler.Handle(new AutoAccrualFeedbackGenerationPayload { CorrelationId = correlationId, MessageId = messageId }, CancellationToken.None));

            var feedback = kafkaPublisher.Feedback;
            Assert.NotNull(feedback);
            Assert.Equal(postingStatus, feedback.PostingDetails!.PostingStatus);
            Assert.Equal(2500, feedback.MileDetails!.BaseMiles);
            Assert.Equal(errorCode, feedback.ErrorInformation?.ErrorDetails.SingleOrDefault()?.ErrorCode);
            Assert.Null(feedback.ErrorInformation?.ErrorDetails.SingleOrDefault()?.ErrorDescription);
            Assert.Equal(Constants.Application.AutoAccrualFeedbackSentStatus, processedRecord.ProcessingStatus);
            Assert.Empty(unprocessedRecord.ProcessingStatus);
            Assert.NotNull(feedbackEventStore.InsertedEntity);
            Assert.Equal(Constants.Application.AutoAccrualFeedbackOutboundType, feedbackEventStore.InsertedEntity!.PartitionKey);
            Assert.Equal(messageId, feedbackEventStore.InsertedEntity.RowKey);
            var storedEvent = JObject.Parse(feedbackEventStore.InsertedEntity.PayLoad);
            Assert.Equal(correlationId, storedEvent.Value<string>("correlationId"));
            Assert.Equal(messageId, storedEvent.Value<string>("id"));
            Assert.Equal(Constants.Application.FeedbackDbaSchemaId, storedEvent.SelectToken("data.schemaId")?.Value<string>());
            Assert.Equal(messageId, kafkaPublisher.MessageId);

            processedRecord.ProcessingStatus = processedStatus;
            Assert.True(await generationHandler.Handle(new AutoAccrualFeedbackGenerationPayload { CorrelationId = correlationId, MessageId = messageId }, CancellationToken.None));
            Assert.Equal(1, kafkaPublisher.PublishCount);
        }

        private static SkyTeamInboundDataEntity CreateEventStoreRecord(string correlationId, string messageId) => new()
        {
            PartitionKey = correlationId,
            RowKey = messageId,
            ParsedAccrualRequest = Json.Serialize(new DistanceBasedAccrualPayload
            {
                Producer = "SK",
                Consumer = "VS",
                RequestType = "AA01",
                AccrualType = "DBA",
                UniqueIdentification = new UniqueIdentification { UniqueIdentificationNumber = "SK123", AirlineCode = "SK" },
                ProductCode = "FLT"
            })
        };

        private sealed class FakeSkyTeamAutoAccrualFeedbackKafkaPublisher : ISkyTeamAutoAccrualFeedbackKafkaPublisher
        {
            public DistanceBasedAccrualFeedbackPayload? Feedback { get; private set; }
            public string? MessageId { get; private set; }
            public int PublishCount { get; private set; }

            public Task<string> PublishAsync(DistanceBasedAccrualFeedbackPayload feedback, string correlationId, string messageId, CancellationToken cancellationToken)
            {
                Feedback = feedback;
                MessageId = messageId;
                PublishCount++;
                return Task.FromResult(messageId);
            }
        }

        private sealed class FakeFeedbackGenerationPublisher : IAutoAccrualFeedbackGenerationPublisher
        {
            public List<(string CorrelationId, string MessageId)> Messages { get; } = [];

            public Task PublishAsync(string correlationId, string messageId, CancellationToken cancellationToken)
            {
                Messages.Add((correlationId, messageId));
                return Task.CompletedTask;
            }
        }

        private sealed class FakeFeedbackEventStore : ITableStorageService<EventStoreDatabaseEntity>, ITableStorageInsertService
        {
            public EventStoreDatabaseEntity? InsertedEntity { get; private set; }
            public EventStoreDatabaseEntity? DeletedEntity { get; private set; }
            public string TableName { get; set; } = string.Empty;
            public string ConnectionString { get; set; } = string.Empty;

            public Task<EventStoreDatabaseEntity> InsertOrMergeAsync(EventStoreDatabaseEntity entity)
            {
                InsertedEntity = entity;
                return Task.FromResult(entity);
            }

            public Task<bool> TryInsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken) where T : TableEntity
            {
                if (InsertedEntity is not null)
                {
                    return Task.FromResult(false);
                }

                InsertedEntity = entity as EventStoreDatabaseEntity ?? throw new ArgumentException("Expected an event-store entity.", nameof(entity));
                return Task.FromResult(true);
            }

            public Task<EventStoreDatabaseEntity> RetrieveAsync(string partitionKey, string rowKey) => throw new NotSupportedException();
            public Task<EventStoreDatabaseEntity> DeleteAsync(EventStoreDatabaseEntity entityToDelete)
            {
                DeletedEntity = entityToDelete;
                return Task.FromResult(entityToDelete);
            }
            public Task<List<EventStoreDatabaseEntity>> RetrieveItemsAsync(TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<EventStoreDatabaseEntity>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<EventStoreDatabaseEntity>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken) => throw new NotSupportedException();
        }

        private sealed class FakeTableStorage(params SkyTeamInboundDataEntity[] entities) : ITableStorageService<SkyTeamInboundDataEntity>
        {
            public string TableName { get; set; } = string.Empty;
            public string ConnectionString { get; set; } = string.Empty;

            public SkyTeamInboundDataEntity Get(string partitionKey, string rowKey) => entities.Single(entity => entity.PartitionKey == partitionKey && entity.RowKey == rowKey);

            public Task<SkyTeamInboundDataEntity> RetrieveAsync(string partitionKey, string rowKey) =>
                Task.FromResult(entities.SingleOrDefault(entity => entity.PartitionKey == partitionKey && entity.RowKey == rowKey)!);

            public Task<SkyTeamInboundDataEntity> InsertOrMergeAsync(SkyTeamInboundDataEntity updatedEntity)
            {
                return Task.FromResult(updatedEntity);
            }

            public Task<SkyTeamInboundDataEntity> DeleteAsync(SkyTeamInboundDataEntity entityToDelete) => throw new NotSupportedException();
            public Task<List<SkyTeamInboundDataEntity>> RetrieveItemsAsync(TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<SkyTeamInboundDataEntity>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<SkyTeamInboundDataEntity>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken) =>
                Task.FromResult(entities.Where(entity => entity.PartitionKey == partitionKey).ToList());
        }
    }
}
