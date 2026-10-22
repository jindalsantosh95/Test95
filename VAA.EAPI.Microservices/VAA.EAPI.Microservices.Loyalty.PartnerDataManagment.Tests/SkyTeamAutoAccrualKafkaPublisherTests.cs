using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class SkyTeamAutoAccrualKafkaPublisherTests
    {
        [Fact]
        public async Task PublishAsync_StoresTheEnvelopeBeforeEncryptingItsPayloadForKafka()
        {
            ConfigReader.Set(Constants.Environment.SkyTeamPgpPublicKey, "test-public-key");
            ConfigReader.Set(Constants.Environment.AccrualPgpEncryptionKeyId, "test-key-id");
            ConfigReader.Set(Constants.Environment.AccrualOutboundKafkaTopic, "test-topic");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceDBKey, "UseDevelopmentStorage=true");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceEventStoreTableName, "EventStore");

            var tableStorage = new FakeEventStoreTableStorage();

            var eventStoreInsertHandler = new EventStoreRequestInsertCommandHandler(
                tableStorage,
                NullLogger<EventStoreRequestInsertCommandHandler>.Instance);
            var eventStoreDeleteHandler = new EventStoreRequestDeleteCommandHandler(
                tableStorage,
                NullLogger<EventStoreRequestDeleteCommandHandler>.Instance);
            var encryptionService = new CapturingEncryptionService();
            var kafkaProducerService = new CapturingKafkaProducerService();
            var publisher = new SkyTeamAutoAccrualKafkaPublisher(
                encryptionService,
                kafkaProducerService,
                eventStoreInsertHandler,
                eventStoreDeleteHandler);

            await publisher.PublishAsync(CreatePayloadWithUnavailableOptionalFields(), CancellationToken.None);

            var innerPayload = JObject.Parse(encryptionService.Plaintext!);
            Assert.Null(innerPayload.SelectToken("uniqueIdentification.airlineCode"));
            Assert.Null(innerPayload.SelectToken("flightDetails.operatingFlight.operatingFlightNumber"));
            Assert.Null(innerPayload["flightTypeIndicator"]);
            Assert.Null(innerPayload.SelectToken("bookingDetails.operatingCarrierFareType"));
            Assert.Null(innerPayload.SelectToken("bookingDetails.marketingCarrierFareType"));
            Assert.Null(innerPayload.SelectToken("documentDetails.documentCouponNumber"));
            Assert.Null(innerPayload["additionalPassengerInformation"]);
            Assert.Null(innerPayload["additionalInformation"]);
            Assert.Null(innerPayload["creditCode"]);
            Assert.Null(innerPayload.SelectToken("documentDetails.documentIssueDateTime"));

            var envelope = JObject.Parse(kafkaProducerService.Message!);
            Assert.Null(envelope["producerSignature"]);
            Assert.Equal(CapturingEncryptionService.EncryptedPayload, envelope.Value<string>("encryptedPayload"));
            Assert.NotNull(tableStorage.InsertedEntity);
            var storedEvent = JObject.Parse(tableStorage.InsertedEntity!.PayLoad);
            var storedEnvelope = Assert.IsType<JObject>(storedEvent["data"]);
            var storedAccrualRequest = JObject.Parse(storedEnvelope.Value<string>("encryptedPayload")!);
            Assert.True(JToken.DeepEquals(innerPayload, storedAccrualRequest));
            Assert.Equal(envelope.Value<string>("messageId"), storedEnvelope.Value<string>("messageId"));
            Assert.Null(storedEnvelope["producerSignature"]);
            Assert.Contains("1101721544", tableStorage.InsertedEntity.PayLoad);
            Assert.DoesNotContain(CapturingEncryptionService.EncryptedPayload, tableStorage.InsertedEntity.PayLoad);
        }

        [Fact]
        public async Task PublishAsync_RemovesStoredEnvelope_WhenEncryptionFails()
        {
            ConfigReader.Set(Constants.Environment.SkyTeamPgpPublicKey, "test-public-key");
            ConfigReader.Set(Constants.Environment.AccrualPgpEncryptionKeyId, "test-key-id");
            ConfigReader.Set(Constants.Environment.AccrualOutboundKafkaTopic, "test-topic");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceDBKey, "UseDevelopmentStorage=true");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceEventStoreTableName, "EventStore");

            var tableStorage = new FakeEventStoreTableStorage();
            var eventStoreInsertHandler = new EventStoreRequestInsertCommandHandler(
                tableStorage,
                NullLogger<EventStoreRequestInsertCommandHandler>.Instance);
            var eventStoreDeleteHandler = new EventStoreRequestDeleteCommandHandler(
                tableStorage,
                NullLogger<EventStoreRequestDeleteCommandHandler>.Instance);
            var publisher = new SkyTeamAutoAccrualKafkaPublisher(
                new ThrowingEncryptionService(),
                new CapturingKafkaProducerService(),
                eventStoreInsertHandler,
                eventStoreDeleteHandler);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => publisher.PublishAsync(CreatePayloadWithUnavailableOptionalFields(), CancellationToken.None));

            Assert.NotNull(tableStorage.InsertedEntity);
            Assert.NotNull(tableStorage.DeletedEntity);
            Assert.Equal(tableStorage.InsertedEntity!.RowKey, tableStorage.DeletedEntity!.RowKey);
            Assert.Equal(tableStorage.InsertedEntity.PartitionKey, tableStorage.DeletedEntity.PartitionKey);
        }

        private static DistanceBasedAccrualPayload CreatePayloadWithUnavailableOptionalFields() => new()
        {
            SchemaId = "nla.accrual.dba",
            SchemaVersion = 1,
            Producer = "VS",
            Consumer = "VS",
            RequestType = "AA01",
            AccrualType = "DBA",
            UniqueIdentification = new UniqueIdentification
            {
                UniqueIdentificationNumber = "12322",
                AirlineCode = ""
            },
            PassengerName = new NameDetails
            {
                SurName = "LUMSDEN",
                GivenName = "ERIN"
            },
            FfpDetails = new FfpDetails
            {
                FfpCarrierCode = "VS",
                FfpAccountNumber = "1101721544"
            },
            FlightDetails = new FlightDetails
            {
                MarketingFlight = new MarketingFlight
                {
                    MarketingCarrierCode = "VS",
                    MarketingFlightNumber = "0209"
                },
                OperatingFlight = new OperatingFlight
                {
                    OperatingCarrierCode = "VS",
                    OperatingFlightNumber = ""
                }
            },
            ScheduledDepartureDate = "2026-10-01",
            FlightTypeIndicator = "",
            FlightSegment = new FlightSegment
            {
                Origin = "LHR",
                Destination = "ICN"
            },
            BookingDetails = new BookingClassDetails
            {
                CabinClassCode = "Y",
                OperatingCarrierBookingClass = "Y",
                OperatingCarrierFareType = "",
                MarketingCarrierBookingClass = "Y",
                MarketingCarrierFareType = ""
            },
            DocumentDetails = new DocumentDetails
            {
                DocumentNumber = "9322118662645",
                DocumentIssueDateTime = ""
            },
            AdditionalPassengerInformation = "",
            CreditCode = "RV",
            ProductCode = "FLT",
            AdditionalInformation = ""
        };

        private sealed class CapturingEncryptionService : IPgpEncryptionService
        {
            public const string EncryptedPayload = "ciphertext";
            public string? Plaintext { get; private set; }

            public string Encrypt(string json, string publicKeyContent)
            {
                Plaintext = json;
                return EncryptedPayload;
            }
        }

        private sealed class CapturingKafkaProducerService : IKafkaProducerService
        {
            public string? Message { get; private set; }

            public Task PublishAsync(string encryptedPayload, CancellationToken cancellationToken)
            {
                Message = encryptedPayload;
                return Task.CompletedTask;
            }
        }

        private sealed class ThrowingEncryptionService : IPgpEncryptionService
        {
            public string Encrypt(string json, string publicKeyContent) => throw new InvalidOperationException("Encryption failed.");
        }

        private sealed class FakeEventStoreTableStorage : ITableStorageService<EventStoreDatabaseEntity>
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

            public Task<EventStoreDatabaseEntity> RetrieveAsync(string partitionKey, string rowKey) => throw new NotSupportedException();
            public Task<EventStoreDatabaseEntity> DeleteAsync(EventStoreDatabaseEntity entity)
            {
                DeletedEntity = entity;
                return Task.FromResult(entity);
            }
            public Task<List<EventStoreDatabaseEntity>> RetrieveItemsAsync(TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<EventStoreDatabaseEntity>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken) => throw new NotSupportedException();
            public Task<List<EventStoreDatabaseEntity>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken) => throw new NotSupportedException();
        }
    }
}
