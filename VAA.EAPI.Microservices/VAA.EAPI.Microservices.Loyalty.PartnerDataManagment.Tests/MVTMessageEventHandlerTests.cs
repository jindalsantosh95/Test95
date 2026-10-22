using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public class MVTMessageEventHandlerTest
    {
        private readonly IServiceProvider _serviceProvider;

        public MVTMessageEventHandlerTest()
        {
            ConfigReader.Set(Constants.Environment.ServiceBusConnectionString, "testConnectionString");
            ConfigReader.Set(Constants.Environment.OrderMSServiceBusConnectionString, "testOrderConnectionString");
            ConfigReader.Set(Constants.Environment.MicroServiceDBKey, "xxx");
            ConfigReader.Set(Constants.Environment.MicroServiceEventStoreTableName, "test");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<IMessageBus>(azureServiceBusFactory.GetClient("testConnectionString", "testSource"))
                .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<EventStoreRequestInsertCommandHandler>()
                .AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<IAzureServiceBusTopicClient, AzureServiceBusTopicClient>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                .AddSingleton<IMVTMessageEventHandler, MVTMessageEventHandler>()
                .AddScoped(typeof(ITableStorageService<>), typeof(VAA.EAPI.Core.Xunit.Azure.TableStorageService<>))
                .BuildServiceProvider();
        }

        [Fact]
        public async Task MVTMessageEventHandler_NullRequest_ReturnsFalse()
        {
            var handler =
                _serviceProvider.GetRequiredService<IMVTMessageEventHandler>();

            var result = await handler.Handle(null);

            Assert.False(result);
        }

        [Fact]
        public async Task MVTMessageEventHandler_ValidRequest_ReturnsTrue()
        {
            var handler =
                _serviceProvider.GetRequiredService<IMVTMessageEventHandler>();

            var request = new MVTMessageRequest
            {
                FlightCarrier = "VS",
                FlightNumber = 222,
                ArrivalAirport = "LHR",
                DepartureAirport = "JFK",
                DepartureDate = "2011-11-02"
            };

            var result = await handler.Handle(request);

            Assert.True(result);
        }
    }
}