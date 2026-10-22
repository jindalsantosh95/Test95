using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.EnrolmentCustomer;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests.ManageAccount
{
    public class UpdateMemberStatusCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly Mock<IMediator> _mediator;
        private readonly ILogger<UpdateMemberStatusCommandHandler> _logger;
        private readonly ILogger<CustomerSearchQueryHandler> _custsearchlogger;
        private readonly ILogger<CustomerRepoUpdateCommandHandler> _custRepologger;
        private readonly ILogger<EventStoreRequestInsertCommandHandler> _eventInsertLogger;
        private readonly ILogger<EventStoreRequestDeleteCommandHandler> _eventDeleteLogger;
        private readonly ILogger<CustomerService<CustomersDTO>> _custServicelogger;
        private readonly UpdateMemberStatusCommandHandler _handler;
        private readonly CustomerRepoUpdateCommandHandler _customerRepoUpdateCommandHandler;
        private readonly CustomerSearchQueryHandler _customerSearchQueryHandler;
        private readonly CustomerDucomentQueryHandler _customerDucomentQueryHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
       private readonly ServiceBusTopicMessageEventHandler _serviceBusTopicMessageEventHandler;
        private readonly IAzureServiceBusTopicClient _serviceBusClient;      
        private readonly IServiceProvider _serviceProvider;
        private readonly CustomerService<CustomersDTO> _customerServiceclass;
        public UpdateMemberStatusCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            var mockmediator = new Mock<IMediator>();
            ConfigReader.Set("MicroServiceDBKey", "testDBKey");
            ConfigReader.Set("MicroServiceEventStoreTableName", "testtblname");
            ConfigReader.Set("OrderServiceBusConnectionString", "testConn");
            ConfigReader.Set("CheckinNotificationTopicName", "testTopic");

            var azureServiceBusFactory = new MessageBusFactory();

            _serviceProvider = new ServiceCollection().AddLogging()                                                     
                                                      .AddSingleton<IMessageBusFactory, MessageBusFactory>()                                                      
                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                                                      .AddSingleton<IRequestHandler<EventRequestInsertCommand, bool>, EventStoreRequestInsertCommandHandler>()
                                                      .AddSingleton<IRequestHandler<EventStoreRequestDeleteCommand, bool>, EventStoreRequestDeleteCommandHandler>()
                                                      .AddSingleton<IRequestHandler<ServiceBusTopicMessageEventCommand, Unit>, ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<IUpdateMemberStatusCommand, UpdateMemberStatusCommandHandler>()
                                                      .BuildServiceProvider();

            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<UpdateMemberStatusCommandHandler>.Instance;
            _custsearchlogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomerSearchQueryHandler>.Instance;
            _custServicelogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomerService<CustomersDTO>>.Instance;
            _custRepologger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomerRepoUpdateCommandHandler>.Instance;
            _customerServiceclass = new CustomerService<CustomersDTO>(mockmediator.Object);
            _customerSearchQueryHandler = new CustomerSearchQueryHandler(mockmediator.Object, _custsearchlogger);
            _customerDucomentQueryHandler = new CustomerDucomentQueryHandler(mockmediator.Object);
            _customerRepoUpdateCommandHandler = new CustomerRepoUpdateCommandHandler(_customerServiceclass, _custRepologger); 
            _eventStoreRequestInsertCommandHandler = (EventStoreRequestInsertCommandHandler?)_serviceProvider.GetService<IRequestHandler<EventRequestInsertCommand, bool>>();
            _eventStoreRequestDeleteCommandHandler = (EventStoreRequestDeleteCommandHandler?)_serviceProvider.GetService<IRequestHandler<EventStoreRequestDeleteCommand, bool>>();
            _serviceBusTopicMessageEventHandler = new ServiceBusTopicMessageEventHandler(_serviceBusClient);
            _handler = new UpdateMemberStatusCommandHandler(_logger, _customerSearchQueryHandler, _customerDucomentQueryHandler, _customerRepoUpdateCommandHandler, _eventStoreRequestInsertCommandHandler, _eventStoreRequestDeleteCommandHandler, _serviceBusTopicMessageEventHandler);
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<System.ArgumentNullException>(() => new UpdateMemberStatusCommandHandler(_logger, _customerSearchQueryHandler, _customerDucomentQueryHandler, _customerRepoUpdateCommandHandler, null, _eventStoreRequestDeleteCommandHandler, _serviceBusTopicMessageEventHandler));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenMediatorObjectIsNull()
        {
            Assert.Throws<System.ArgumentNullException>(() => new UpdateMemberStatusCommandHandler(null, _customerSearchQueryHandler, _customerDucomentQueryHandler, _customerRepoUpdateCommandHandler, _eventStoreRequestInsertCommandHandler, _eventStoreRequestDeleteCommandHandler, _serviceBusTopicMessageEventHandler));
        }

        [Fact]
        public async Task UpdateMemberStatusCommandHandler_ValidScenario()
        {
            var bulkMemberStatusUpdateRequest = new BulkMemberStatusUpdateRequest
            {
                MemberNumber = new[] { "15401849017" },
                MemberStatus = "suspended"
            };
            var result = await _handler.Handle(bulkMemberStatusUpdateRequest, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task UpdateMemberStatusCommandHandler_ErrorScenario()
        {
            var bulkMemberStatusUpdateRequest = (BulkMemberStatusUpdateRequest)new BulkMemberStatusUpdateRequest();

            var result = await _handler.Handle(bulkMemberStatusUpdateRequest, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("G009", result.Value.Errors.First().Code);
            Assert.Equal("There was an unexpected error.", result.Value.Errors.First().Message);

        }
    }
}
