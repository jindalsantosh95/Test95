using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.HouseHoldLink;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public class HouseHoldAccountEmailTriggerCommandHandlerTest
    {
        private readonly VAA.EAPI.Core.Xunit.ServiceBus.MessageBusFactory _messageBusFactory = new();
        private readonly IServiceProvider _serviceProvider;

        public HouseHoldAccountEmailTriggerCommandHandlerTest()
        {
            MemberTestHelpers.CreateTestEnvironmentSettings();
            ConfigReader.Set("MicroServiceDBKey", "UseDevelopmentStorage=true");
            ConfigReader.Set("MicroServiceEventStoreTableName", "TestEventStore");
            ConfigReader.Set("ServiceBusConnectionString", "testConnectionString");
            _serviceProvider = new ServiceCollection().AddLogging().AddSingleton<IMessageBusFactory>(_messageBusFactory)
                .AddSingleton(typeof(VAA.EAPI.Infrastructure.AzureStorage.ITableStorageService<>), typeof(VAA.EAPI.Core.Xunit.Azure.TableStorageService<>))
                .AddSingleton<EventStoreRequestInsertCommandHandler>().AddSingleton<EventStoreRequestDeleteCommandHandler>()
                .AddSingleton<ServiceBusTopicMessageEventCommandHandler>().AddSingleton<HouseHoldAccountEmailTriggerCommandHandler>().BuildServiceProvider();
        }

        [Fact]
        public async Task HandlerSuccess()
        {
            var result = await Handler().Handle(CreateRequest(), CancellationToken.None);
            Assert.True(result);
        }

        [Fact]
        public async Task HandlerReturnsFalseAndCompensatesWhenPublicationFails()
        {
            ConfigReader.Set("ServiceBusConnectionString", string.Empty);
            var result = await Handler().Handle(CreateRequest(), CancellationToken.None);
            ConfigReader.Set("ServiceBusConnectionString", "testConnectionString");
            Assert.False(result);
        }

        [Fact]
        public async Task YoungFlyerEmailIsPersistedAndPublished()
        {
            var published = new System.Collections.Concurrent.ConcurrentBag<VAA.EAPI.Domain.Events.CloudEvent>();
            var bus = (VAA.EAPI.Core.Xunit.ServiceBus.MessageBus)_messageBusFactory.GetClient("testConnectionString", "householdaccount");
            bus.OnMessage += published.Add;
            var result = await Handler().Handle(new HouseHoldAccountEmailTriggerCommand
            {
                Id = Guid.NewGuid(), FirstName = "Parent", Email = "parent@example.com",
                EmailTemplate = "YoungFlyerEnrolmentConfirmationEmailTemplate", ChildFirstName = "Child",
                ChildLastName = "ChildLast", ChildMemberNumber = "1000000002", IsYoungFlyerParentEmail = true
            }, CancellationToken.None);

            var publishedEvent = Assert.Single(published);
            Assert.True(result);
            Assert.Equal("InviteEmailEvent", publishedEvent.Type);
            Assert.Equal("parent@example.com", ((YoungFlyerParentEmailCommand)publishedEvent.Data).Email);
        }

        private static HouseHoldAccountEmailTriggerCommand CreateRequest() => new()
        {
            Id = Guid.NewGuid(), FirstName = "First", LastName = "Last", MemberNumber = "Member001",
            CustomerId = "Customer001", Email = "Customer001@mail.com", EmailTemplate = "HoHLinkHeadSilverEmalTemplate"
        };

        private HouseHoldAccountEmailTriggerCommandHandler Handler() => _serviceProvider.GetRequiredService<HouseHoldAccountEmailTriggerCommandHandler>();

    }
}
