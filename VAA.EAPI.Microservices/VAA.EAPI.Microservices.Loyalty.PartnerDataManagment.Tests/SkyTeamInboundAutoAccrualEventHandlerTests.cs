using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.TableStorage.Interface;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class SkyTeamInboundAutoAccrualEventHandlerTests
    {
        private const string DecryptedPayload = "{\"schemaId\":\"nla.accrual.dba\",\"schemaVersion\":1}";
        private const string EncryptedPayload = "dGVzdC1lbmNyeXB0ZWQtcGF5bG9hZA==";
        private readonly IServiceProvider _serviceProvider;
        private readonly DecryptingPgpService _pgpDecryptionService;
        private readonly ITableStorageService<SkyTeamInboundDataEntity> _inboundDataTable;
        private readonly MessageLookupStore _messageLookupStore;

        public SkyTeamInboundAutoAccrualEventHandlerTests()
        {
            ConfigureSettings();
            _pgpDecryptionService = new DecryptingPgpService();
            _messageLookupStore = new MessageLookupStore();
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IPgpDecryptionService>(_pgpDecryptionService)
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(VAA.EAPI.Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<ITableStorageInsertService>(_messageLookupStore)
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ISkyTeamInboundAutoAccrualEventHandler, SkyTeamInboundAutoAccrualEventHandler>()
                .BuildServiceProvider();
            _inboundDataTable = _serviceProvider.GetRequiredService<ITableStorageService<SkyTeamInboundDataEntity>>();
        }

        [Fact]
        public async Task Handle_ValidMessage_StoresDecryptedPayload()
        {
            var correlationId = Guid.NewGuid();
            var messageId = Guid.NewGuid().ToString();

            Assert.True(await Handler.Handle(CreateEnvelope(correlationId, messageId), TestContext.Current.CancellationToken));

            var storedData = await _inboundDataTable.RetrieveAsync(correlationId.ToString(), messageId);
            Assert.Equal(DecryptedPayload, storedData.DecryptedPayload);
            Assert.Equal(Constants.Application.ClaimStatus, storedData.ProcessingStatus);
            Assert.Null(storedData.ErrorCode);
            Assert.Equal(correlationId, _messageLookupStore.GetCorrelationId(messageId));
        }

        [Fact]
        public async Task Handle_EnvelopeForAnotherConsumer_StoresError()
        {
            var correlationId = Guid.NewGuid();
            var messageId = Guid.NewGuid().ToString();
            var envelope = Json.Deserialize<NlaEnvelopeBase>(CreateEnvelope(correlationId, messageId));
            envelope!.Consumer = "AA";

            Assert.True(await Handler.Handle(Json.Serialize(envelope), TestContext.Current.CancellationToken));

            var storedData = await _inboundDataTable.RetrieveAsync(correlationId.ToString(), messageId);
            Assert.Equal(Constants.Application.ErrorStatus, storedData.ProcessingStatus);
            Assert.Equal(NlaErrorCodes.InvalidEnvelopeConsumerFormat, storedData.ErrorCode);
        }

        [Fact]
        public async Task Handle_DecryptionFailureWithValidMessageId_StoresError()
        {
            _pgpDecryptionService.ExceptionToThrow = new InvalidOperationException("Unable to decrypt.");
            var correlationId = Guid.NewGuid();
            var messageId = Guid.NewGuid().ToString();

            Assert.True(await Handler.Handle(CreateEnvelope(correlationId, messageId), TestContext.Current.CancellationToken));

            var storedData = await _inboundDataTable.RetrieveAsync(correlationId.ToString(), messageId);
            Assert.Equal(Constants.Application.ErrorStatus, storedData.ProcessingStatus);
            Assert.Equal(NlaErrorCodes.PgpDecryptionFailed, storedData.ErrorCode);
        }

        [Fact]
        public async Task Handle_UnparseableEnvelope_LogsAndSkipsWithoutCreatingRecords()
        {
            Assert.False(await Handler.Handle("not-json", TestContext.Current.CancellationToken));

            Assert.Equal(0, _messageLookupStore.Count);
        }

        [Fact]
        public async Task Handle_DuplicateMessageId_DoesNotCreateAnotherLookupRecord()
        {
            var correlationId = Guid.NewGuid();
            var messageId = Guid.NewGuid().ToString();
            var message = CreateEnvelope(correlationId, messageId);

            Assert.True(await Handler.Handle(message, TestContext.Current.CancellationToken));
            Assert.True(await Handler.Handle(message, TestContext.Current.CancellationToken));
            Assert.Equal(1, _messageLookupStore.Count);
        }

        private ISkyTeamInboundAutoAccrualEventHandler Handler => _serviceProvider.GetRequiredService<ISkyTeamInboundAutoAccrualEventHandler>();

        private static string CreateEnvelope(Guid correlationId, string messageId) => Json.Serialize(new NlaEnvelopeBase { SchemaId = Constants.Application.AccrualDbaSchemaId, SchemaVersion = Constants.Application.AccrualDbaSchemaVersion, Producer = "SK", Consumer = "VS", Timestamp = "2026-09-16T10:00:00Z", MessageId = messageId, CorrelationId = correlationId.ToString(), EncryptionKeyId = "skyteam-key", EncryptedPayload = EncryptedPayload });

        private static void ConfigureSettings()
        {
            ConfigReader.Set(Constants.Environment.VsPgpPrivateKey, "test-private-key");
            ConfigReader.Set(Constants.Environment.ServiceBusConnectionString, "test-service-bus-connection");
            ConfigReader.Set(Constants.Environment.MicroServiceDBKey, "test-storage-connection");
            ConfigReader.Set(Constants.Environment.MicroServiceEventStoreTableName, "DefaultEventStore");
            ConfigReader.Set(Constants.Environment.SkyTeamInboundDataTableName, "SkyTeamInboundAccrualData");
            ConfigReader.Set(Constants.Environment.SkyTeamInboundAccrualMessageLookupTableName, "SkyTeamInboundAccrualMessageLookup");
        }

        private class DecryptingPgpService : IPgpDecryptionService
        {
            public Exception? ExceptionToThrow { get; set; }

            public string Decrypt(string encryptedPayload, string privateKey)
            {
                if (ExceptionToThrow is not null)
                {
                    throw ExceptionToThrow;
                }

                return DecryptedPayload;
            }
        }

        private class MessageLookupStore : ITableStorageInsertService
        {
            private readonly Dictionary<string, Guid> _messageIds = [];

            public Task<bool> TryInsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken) where T : Microsoft.Azure.Cosmos.Table.TableEntity
            {
                cancellationToken.ThrowIfCancellationRequested();
                var messageLookupEntity = Assert.IsType<SkyTeamNlaMessageLookupEntity>(entity);
                return Task.FromResult(_messageIds.TryAdd(messageLookupEntity.RowKey, Guid.Parse(messageLookupEntity.CorrelationId)));
            }

            public Guid GetCorrelationId(string messageId) => _messageIds[messageId];

            public int Count => _messageIds.Count;
        }

    }
}
