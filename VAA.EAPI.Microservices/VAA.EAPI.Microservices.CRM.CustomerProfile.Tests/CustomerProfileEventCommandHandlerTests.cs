using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Functions;
using VAA.EAPI.Core.Xunit.Azure;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Events;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Tests
{
    public class CustomerProfileEventCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomerProfileEventCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testtblname");
            ConfigReader.Set("OrderServiceBusConnectionString", "testConn");
            ConfigReader.Set("CheckinNotificationTopicName", "testTopic");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()

                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))

                                                      .AddSingleton<IAmperityApiRepoQuery, AmperityApiRepoQueryHandler>()
                                                      .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>()

                                                      .AddSingleton<ICustomerProfileEventCommand, CustomerProfileEventCommandHandler>()

                                                      .BuildServiceProvider();
        }

        [Fact]
        public void HandlerShouldThrowException_WhenConstructorParameterIsNull()
        {
            var amperityApiRepoQueryHandler = _serviceProvider.GetService<IAmperityApiRepoQuery>();
            var eventStoreRequestInsertCommandHandler = _serviceProvider.GetService<IRequestHandler<EventRequestInsertCommand, bool>>();
            var eventStoreRequestDeleteCommandHandler = _serviceProvider.GetService<IRequestHandler<EventStoreRequestDeleteCommand, bool>>();
            var serviceBusTopicMessageEventCommandHandler = _serviceProvider.GetService<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>>();
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomerProfileEventCommandHandler>.Instance;

            Assert.Throws<ArgumentNullException>(() => new CustomerProfileEventCommandHandler(null, eventStoreRequestInsertCommandHandler, eventStoreRequestDeleteCommandHandler, serviceBusTopicMessageEventCommandHandler, logger));
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileEventCommandHandler(amperityApiRepoQueryHandler, null, eventStoreRequestDeleteCommandHandler, serviceBusTopicMessageEventCommandHandler, logger));
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileEventCommandHandler(amperityApiRepoQueryHandler, eventStoreRequestInsertCommandHandler, null, serviceBusTopicMessageEventCommandHandler, logger));
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileEventCommandHandler(amperityApiRepoQueryHandler, eventStoreRequestInsertCommandHandler, eventStoreRequestDeleteCommandHandler, null, logger));
            Assert.Throws<ArgumentNullException>(() => new CustomerProfileEventCommandHandler(amperityApiRepoQueryHandler, eventStoreRequestInsertCommandHandler, eventStoreRequestDeleteCommandHandler, serviceBusTopicMessageEventCommandHandler, null));
        }

        [Fact]
        public async Task Handle_Should_Success()
        {
            // Arrange
            FunctionContext functionContext = new TestFunctionContext();
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            var customerProfileHandler = _serviceProvider.GetService<ICustomerProfileEventCommand>()!;
            var tableStorageService = (Core.Xunit.Azure.TableStorageService<EventStoreDatabaseEntity>)_serviceProvider.GetService<ITableStorageService<EventStoreDatabaseEntity>>()!;
            var messageBusFactory = _serviceProvider.GetService<IMessageBusFactory>()!;
            var messageBus = (MessageBus)messageBusFactory.GetClient(ConfigReader.Get("OrderServiceBusConnectionString"), ConfigReader.Get("CheckinNotificationTopicName"));

            EAPI.Domain.Events.CloudEvent? message = default;

            // capture message sent to service bus for validation
            messageBus.OnMessage += (m) => message = m;

            var flightOrderDataList = new List<FlightOrderData>
            {
                new()
                {
                    BookingReference = new()
                    {
                        PnrLocator = "PNR123"
                    },
                    Passengers =
                    [
                        new()
                        {
                            FirstName = "John",
                            LastName = "Mathew",
                            PaxType = "TypePax",                            
                        }
                    ]
                }
            };

            var cloudEvent = new EAPI.Domain.Events.CloudEvent
            {
                Data = Json.Serialize(flightOrderDataList)
            };

            // Act

            // we need to wait till this task has finished executing
            customerProfileHandler.Handle(cloudEvent, CancellationToken.None)
                                  .GetAwaiter()
                                  .GetResult();

            //Assert
            var soure = flightOrderDataList.First();
            var sourcePassenger = soure.Passengers.First();


            // validate data sent to table storage
            Assert.Equal(1, tableStorageService.Count);

            var record = (await tableStorageService.RetrieveItemsAsync(new Microsoft.Azure.Cosmos.Table.TableContinuationToken())).First()
                                                                                                                                  .PayLoad
                                                                                                                                  .Parse<JObject>();

            var recordData = Json.Deserialize<IEnumerable<FlightOrderData>>(record["data"]!.ToString()).First();
            var recordPassenger = recordData.Passengers.First();

            Assert.Equal(recordData.BookingReference.PnrLocator, soure.BookingReference.PnrLocator);
            Assert.Equal(recordPassenger.FirstName, sourcePassenger.FirstName);
            Assert.Equal(recordPassenger.LastName, sourcePassenger.LastName);
            Assert.Equal(recordPassenger.PaxType, sourcePassenger.PaxType);            


            // validate data sent to service bus
            Assert.Equal(1, messageBus.Delivered);
            Assert.NotNull(message);

            var messageData = ((IEnumerable<FlightOrderData>)message.Data).First();
            var messagePassenger = messageData.Passengers.First();

            Assert.Equal(messageData.BookingReference.PnrLocator, soure.BookingReference.PnrLocator);
            Assert.Equal(messagePassenger.FirstName, sourcePassenger.FirstName);
            Assert.Equal(messagePassenger.LastName, sourcePassenger.LastName);
            Assert.Equal(messagePassenger.PaxType, sourcePassenger.PaxType);            
        }
    }
}
