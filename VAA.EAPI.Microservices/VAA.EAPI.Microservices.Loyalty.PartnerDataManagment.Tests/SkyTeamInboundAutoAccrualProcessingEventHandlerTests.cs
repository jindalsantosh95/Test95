using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class SkyTeamInboundAutoAccrualProcessingEventHandlerTests
    {
        private const string DecryptedPayload = "{\"schemaId\":\"nla.accrual.dba\",\"schemaVersion\":1,\"producer\":\"SK\",\"consumer\":\"VS\",\"requestType\":\"AA01\",\"accrualType\":\"DBA\",\"uniqueIdentification\":{\"uniqueIdentificationNumber\":\"123456789012345678901\"},\"passengerName\":{\"surName\":\"Smith\",\"givenName\":\"Alex\"},\"ffpDetails\":{\"ffpCarrierCode\":\"VS\",\"ffpAccountNumber\":\"1234567890\"},\"flightDetails\":{\"marketingFlight\":{\"marketingCarrierCode\":\"VS\",\"marketingFlightNumber\":\"003\"},\"operatingFlight\":{\"operatingCarrierCode\":\"VS\",\"operatingFlightNumber\":\"003\"}},\"scheduledDepartureDate\":\"2026-09-16\",\"flightSegment\":{\"origin\":\"LHR\",\"destination\":\"JFK\"},\"bookingDetails\":{\"cabinClassCode\":\"J\",\"operatingCarrierBookingClass\":\"J\",\"marketingCarrierBookingClass\":\"J\"},\"documentDetails\":{\"documentNumber\":\"1234567890123\",\"documentCouponNumber\":1},\"productCode\":\"FLT\"}";
        private readonly IServiceProvider _serviceProvider;
        private readonly ITableStorageService<SkyTeamInboundDataEntity> _inboundDataTable;

        public SkyTeamInboundAutoAccrualProcessingEventHandlerTests()
        {
            ConfigureSettings();
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton(typeof(ITableStorageService<>), typeof(VAA.EAPI.Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<SkyTeamInboundAutoAccrualProcessingEventHandler>()
                .BuildServiceProvider();
            _inboundDataTable = _serviceProvider.GetRequiredService<ITableStorageService<SkyTeamInboundDataEntity>>();
        }

        [Fact]
        public async Task Handle_ValidPayload_StoresParsedRequest()
        {
            var correlationId = Guid.NewGuid();
            var messageId = Guid.NewGuid().ToString();
            await _inboundDataTable.InsertOrMergeAsync(CreateInboundData(messageId, correlationId, DecryptedPayload));

            Assert.True(await Handler.Handle(CreateProcessingPayload(correlationId, messageId), TestContext.Current.CancellationToken));

            var storedData = await _inboundDataTable.RetrieveAsync(correlationId.ToString(), messageId);
            Assert.NotEmpty(storedData.ParsedAccrualRequest);
            Assert.Equal(Constants.Application.ClaimStatus, storedData.ProcessingStatus);
            Assert.Null(storedData.ErrorCode);
        }

        [Fact]
        public async Task Handle_InvalidPayload_StoresError()
        {
            var correlationId = Guid.NewGuid();
            var messageId = Guid.NewGuid().ToString();
            const string invalidPayload = "{\"schemaId\":\"nla.accrual.dba\",\"schemaVersion\":1,\"producer\":\"SK\",\"consumer\":\"VS\",\"requestType\":\"AA01\",\"accrualType\":\"DBA\"}";
            await _inboundDataTable.InsertOrMergeAsync(CreateInboundData(messageId, correlationId, invalidPayload));

            Assert.True(await Handler.Handle(CreateProcessingPayload(correlationId, messageId), TestContext.Current.CancellationToken));

            var storedData = await _inboundDataTable.RetrieveAsync(correlationId.ToString(), messageId);
            Assert.Equal(Constants.Application.ErrorStatus, storedData.ProcessingStatus);
            Assert.Equal(NlaErrorCodes.MissingUniqueIdentification, storedData.ErrorCode);
        }

        private SkyTeamInboundAutoAccrualProcessingEventHandler Handler => _serviceProvider.GetRequiredService<SkyTeamInboundAutoAccrualProcessingEventHandler>();

        private static AutoAccrualProcessingPayload CreateProcessingPayload(Guid correlationId, string messageId) => new() { CorrelationId = correlationId, MessageId = messageId };

        private static SkyTeamInboundDataEntity CreateInboundData(string messageId, Guid correlationId, string decryptedPayload) => new() { PartitionKey = correlationId.ToString(), RowKey = messageId, Envelope = CreateEnvelope(correlationId, messageId), DecryptedPayload = decryptedPayload, CorrelationId = correlationId.ToString(), ProcessingStatus = Constants.Application.ClaimStatus };

        private static string CreateEnvelope(Guid correlationId, string messageId) => Json.Serialize(new NlaEnvelopeBase { SchemaId = Constants.Application.AccrualDbaSchemaId, SchemaVersion = Constants.Application.AccrualDbaSchemaVersion, Producer = "SK", Consumer = "VS", Timestamp = "2026-09-16T10:00:00Z", MessageId = messageId, CorrelationId = correlationId.ToString(), EncryptionKeyId = "skyteam-key", EncryptedPayload = "dGVzdC1lbmNyeXB0ZWQtcGF5bG9hZA==" });

        private static void ConfigureSettings()
        {
            ConfigReader.Set(Constants.Environment.ServiceBusConnectionString, "test-service-bus-connection");
            ConfigReader.Set(Constants.Environment.MicroServiceDBKey, "test-storage-connection");
            ConfigReader.Set(Constants.Environment.MicroServiceEventStoreTableName, "DefaultEventStore");
            ConfigReader.Set(Constants.Environment.SkyTeamInboundDataTableName, "SkyTeamInboundAccrualData");
            ConfigReader.Set(Constants.Environment.LoyaltyMemberManagementTopicName, "test-loyalty-member-management-topic");
        }

    }
}
