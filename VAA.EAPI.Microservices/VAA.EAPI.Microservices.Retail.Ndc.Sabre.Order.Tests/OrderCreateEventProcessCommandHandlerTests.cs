using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreatePNR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Application.Commands;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Repositories;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Infrastructure.Entity;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Tests
{
    public class OrderCreateEventProcessCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public OrderCreateEventProcessCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set(Constants.Environment.ConversationId, "ABC123");
            ConfigReader.Set(Constants.Environment.FromPartyId, "1");
            ConfigReader.Set(Constants.Environment.ToPartyId, "2");
            ConfigReader.Set(Constants.Environment.SSRDelayInMS, "0");

            ConfigReader.Set("ArcTransactionClientId", "abc");
            ConfigReader.Set("ArcTransactionClientSecret", "xxx");
            ConfigReader.Set("ArcTransactionTokenUrl", "https://tempuri.org/ArcTransactionToken");
            ConfigReader.Set("ArcTransactionAudience", "arcTransactionScope");
            ConfigReader.Set("TicketingSystemCode", "abcd");
            ConfigReader.Set("ARCEnvironmentName", "Test");

            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceDBKey, "dummy");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceResponseTableName, "dummy-response");
            ConfigReader.Set(VAA.EAPI.Infrastructure.Constants.Constants.MicroServiceEventStoreTableName, "dummy-eventstore");

            ConfigReader.Set(Constants.Environment.OrderDataServiceBusConnectionString, "dummy-conn");
            ConfigReader.Set(Constants.Environment.OrderDataServiceBusTopicName, "dummy-topic");

            var getReservationResponse = DeserializeGetReservationResponse(TestDataProvider.GetXmlTestData<string>("SabreResponses\\GetReservationRS"));

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IMediator>(_ => new FakeMediator(getReservationResponse))
                .AddSingleton<ITableStorageService<ServiceResponseDatabaseEntity>, InMemoryTableStorageService<ServiceResponseDatabaseEntity>>()
                .AddSingleton<ITableStorageService<EventStoreDatabaseEntity>, InMemoryTableStorageService<EventStoreDatabaseEntity>>()
                .AddSingleton<IMessageBusFactory, FakeMessageBusFactory>()
                .AddSingleton<ServiceBusClient>(_ => new ServiceBusClient("Endpoint=sb://unit-test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=AAAAAAAAAAAAAAAAAAAAAA=="))
                .AddSingleton<OrderCreateRepoCommandHandler>()
                .AddSingleton<ServiceResponseInsertCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<OrderCreateEventProcessCommandHandler>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task Handle_ShouldSucceed()
        {
            ConfigReader.Set("ArcTransactionUrl", "https://tempuri.org/ArcTransaction");
            ConfigReader.Set("OrderApiRetryCount", "3");
            ConfigReader.Set("OrderRetryDelayMilliseconds", "1000");

            var handler = _serviceProvider.GetService<OrderCreateEventProcessCommandHandler>()!;

            var request = new OrderCreateEventProcessCommand
            {
                Data = TestDataProvider.GetTestData<OrderCreateRequest>("NDCOrderMS\\OrderCreateRequest")
            };

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }

        private static GetReservationRS DeserializeGetReservationResponse(string xml)
        {
            xml = xml.Replace("msgType=\"0\"", "msgType=\"S\"");

            var serializer = new XmlSerializer(
                typeof(GetReservationRS),
                new XmlRootAttribute("GetReservationRS") { Namespace = "http://webservices.sabre.com/pnrbuilder/v1_19" });

            using var reader = new StringReader(xml);
            return (GetReservationRS)serializer.Deserialize(reader)!;
        }

    }

    internal sealed class FakeMediator(GetReservationRS getReservationResponse) : IMediator
    {
        private readonly GetReservationRS _getReservationResponse = getReservationResponse;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return request switch
            {
                CreateSessionCommand => Task.FromResult((TResponse)(object)"dummyToken"),
                CreatePNRCommand => Task.FromResult((TResponse)(object)new CreatePassengerNameRecordRS
                {
                    ItineraryRef = new CreatePassengerNameRecordRSItineraryRef { ID = "DE6DIF" }
                }),
                GetReservationQuery => Task.FromResult((TResponse)(object)_getReservationResponse),
                SessionCloseCommand => Task.FromResult((TResponse)(object)true),
                _ => throw new InvalidOperationException($"Unsupported request type: {request.GetType().FullName}")
            };
        }

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException($"Unsupported request type: {request.GetType().FullName}");

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            throw new NotSupportedException($"Unsupported request type: {request.GetType().FullName}");

        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification => Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException($"Unsupported request type: {request.GetType().FullName}");

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException($"Unsupported request type: {request.GetType().FullName}");
    }

    internal sealed class InMemoryTableStorageService<T> : ITableStorageService<T> where T : class, ITableEntity
    {
        public string TableName { get; set; } = string.Empty;

        public string ConnectionString { get; set; } = string.Empty;

        public Task<T> RetrieveAsync(string partitionKey, string rowKey) => Task.FromResult<T>(default!);

        public Task<T> InsertOrMergeAsync(T entity) => Task.FromResult(entity);

        public Task<T> DeleteAsync(T entity) => Task.FromResult(entity);

        public Task<List<T>> RetrieveItemsAsync(TableContinuationToken continutaionToken) =>
            Task.FromResult(new List<T>());

        public Task<List<T>> RetrieveItemsAsync(string partitionKey, string columnName, string columnData, TableContinuationToken continutaionToken) =>
            Task.FromResult(new List<T>());

        public Task<List<T>> RetrieveItemsInPartitionAsync(string partitionKey, TableContinuationToken continutaionToken) =>
            Task.FromResult(new List<T>());
    }

    internal sealed class FakeMessageBusFactory : IMessageBusFactory
    {
        public IMessageBus GetClient(string connectionString, string sender) => new FakeMessageBus();

        private sealed class FakeMessageBus : IMessageBus
        {
            public Task PublishMessageAsync(CloudEvent @event) => Task.CompletedTask;
        }
    }
}
