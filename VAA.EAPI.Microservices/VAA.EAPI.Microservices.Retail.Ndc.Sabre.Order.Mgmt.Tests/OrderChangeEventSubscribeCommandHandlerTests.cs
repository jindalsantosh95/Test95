using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.ServiceModel;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Core.Xunit.ServiceBus;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Entity;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.DesignatePrinter;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SabreCommandLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TKTExchange;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class OrderCreateEventProcessCommandHandlerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IMediator _mediator;
        private readonly Mock<ILogger<OrderChangeEventSubscribeCommandHandler>> _logger;
        private readonly OrderChangeEventSubscribeCommandHandler _handler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOrderChangeAcceptTimeChangeDisruptionCommand _timeChangeDisruptionCommand;
        private readonly IOrderChangeAcceptCancellationDisruptionCommand _cancellationDisruptionCommand;
        private readonly IOrderChangeReshopDisruptionCommand _reshopDisruptionCommand;
        private readonly ITableStorageService<ServiceResponseDatabaseEntity> _tableStorage;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly IOrderChangeReshopHoldBookingCommand _orderChangeReshopHoldBookingCommand;

        public OrderCreateEventProcessCommandHandlerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("sabreendpoint", "https://test.com");
            _tableStorage = new Core.Xunit.Azure.TableStorageService<ServiceResponseDatabaseEntity>();
            _logger = new Mock<ILogger<OrderChangeEventSubscribeCommandHandler>>();
            _serviceProvider = new ServiceCollection().AddLogging()
                .AddMediatR(config => {
                    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                    config.RegisterServicesFromAssembly(typeof(OrderChangeRepoCommandHandler).Assembly);
                    config.RegisterServicesFromAssemblyContaining(typeof(CreateSessionCommand));
                    config.RegisterServicesFromAssemblyContaining(typeof(ServiceResponseInsertCommand));
                })
                .AddSingleton<ServiceResponseInsertCommandHandler>()
                .AddSingleton<IMessageBusFactory, MessageBusFactory>()
                .AddSingleton<ITableStorageService<ServiceResponseDatabaseEntity>>(_tableStorage)
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<ICreateSession, CreateSession>()
                                                      .AddSingleton<IGetReservation, TestGetReservation>()
                                                      .AddSingleton<IOTACancelLLS, TestOTACancel>()
                                                       .AddSingleton<IEnhancedAirBook, TestEnhancedAirBook>()
                                                      .AddSingleton<ITKTExchange, TKTExchange>()
                                                      .AddSingleton<ISessionClose, TestSessionClose>()
                                                      .AddSingleton<ISabreCommandLLS, VAA.EAPI.Infrastructure.NDC.Sabre.Services.SabreCommandLLS>()
                                                      .AddSingleton<IEnhancedEndTransaction, TestEnhancedEndTransaction>()
                                                      .AddSingleton<IEnhancedAirTicket, TestEnhancedAirTicket>()
                                                      .AddSingleton<IGetElectronicDocument, TestGetElectronicDocumentQuery>()
                                                      .AddSingleton<IOrderChangeAcceptTimeChangeDisruptionCommand, OrderChangeAcceptTimeChangeDisruptionCommandHandler>()
                                                      .AddSingleton<IOrderChangeAcceptCancellationDisruptionCommand, OrderChangeAcceptCancellationDisruptionCommandHandler>()
                                                      .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton<TestCreateSessionCommandHandler>()
                                                      .AddSingleton<CreateSessionCommandHandler>()
                                                      .AddSingleton<GetReservationQueryHandler>()
                                                      .AddSingleton<SessionCloseCommandHandler>()
                                                      .AddSingleton<SabreCommandLLSCommandHandler>()
                                                      .AddSingleton<EnhancedEndTransactionCommandHandler>()
                                                      .AddSingleton<EnhancedAirTicketCommandHandler>()
                                                      .AddSingleton<DesignatePrinterCommandHandler>()
                                                      .AddSingleton<GetElectronicDocumentQueryHandler>()
                                                      .AddSingleton<ServiceBusTopicMessageEventCommandHandler>()
                                                      .AddSingleton(typeof(ICreateSession), typeof(CreateSession))
                                                      .AddSingleton(typeof(ITableStorageService<>), typeof(Core.Xunit.Azure.TableStorageService<>))
                                                      .AddSingleton(typeof(IDesignatePrinter), typeof(DesignatePrinter))
                                                      .AddSingleton(typeof(IEndTransaction), typeof(EndTransaction))
                                                      .AddSingleton(typeof(ITKTExchange), typeof(TKTExchange))
                                                      .AddSingleton(typeof(ITKTExchangeCommand), typeof(TKTExchangeCommandHandler))
                                                      .AddSingleton(s => new ExchangePortTypeClient(ExchangePortTypeClient.EndpointConfiguration.ExchangePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                      .AddSingleton(s => new SessionCreatePortTypeClient(SessionCreatePortTypeClient.EndpointConfiguration.SessionCreatePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                      .AddSingleton(s => new GetReservationPortTypeClient(GetReservationPortTypeClient.EndpointConfiguration.GetReservationPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                      .AddSingleton(s => new EnhancedEndTransactionPortTypeClient(EnhancedEndTransactionPortTypeClient.EndpointConfiguration.EnhancedEndTransactionPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                      .AddSingleton(s => new SessionClosePortTypeClient(SessionClosePortTypeClient.EndpointConfiguration.SessionClosePortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                      .AddSingleton(s => new DesignatePrinterPortTypeClient(DesignatePrinterPortTypeClient.EndpointConfiguration.DesignatePrinterPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                      .AddSingleton(s => new EndTransactionPortTypeClient(EndTransactionPortTypeClient.EndpointConfiguration.EndTransactionPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                      .AddSingleton(s => new GetElectronicDocumentPortTypeClient(GetElectronicDocumentPortTypeClient.EndpointConfiguration.GetElectronicDocumentPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                     .AddSingleton(s => new SabreCommandPortTypeClient(SabreCommandPortTypeClient.EndpointConfiguration.SabreCommandPortType, new EndpointAddress(new Uri(ConfigReader.Get(Constants.Environment.SabreEndpoint)))))
                                                     .BuildServiceProvider();
            _timeChangeDisruptionCommand = _serviceProvider.GetService<IOrderChangeAcceptTimeChangeDisruptionCommand>()!;
            _cancellationDisruptionCommand = _serviceProvider.GetService<IOrderChangeAcceptCancellationDisruptionCommand>()!;
            _reshopDisruptionCommand = new Mock<IOrderChangeReshopDisruptionCommand>().Object;
            _serviceBusTopicMessageEventCommandHandler = _serviceProvider.GetService<ServiceBusTopicMessageEventCommandHandler>()!;
            _orderChangeReshopHoldBookingCommand = _serviceProvider.GetService<IOrderChangeReshopHoldBookingCommand>()!;
            _mediator = _serviceProvider.GetService<IMediator>()!;
            _handler = new OrderChangeEventSubscribeCommandHandler(_mediator, _logger.Object, _timeChangeDisruptionCommand, _cancellationDisruptionCommand, _reshopDisruptionCommand, _serviceBusTopicMessageEventCommandHandler, _orderChangeReshopHoldBookingCommand);
            ConfigReader.Set("OrderDataServiceBusConnectionString", "dummyConnString");
            ConfigReader.Set("OrderDataServiceBusTopicName", "test");
            ConfigReader.Set("frompartyid", "test");
            ConfigReader.Set("topartyid", "test");
            ConfigReader.Set("conversationid", "test");
            ConfigReader.Set("retrycount", "1");
            ConfigReader.Set("maxretriescount", "1");
            ConfigReader.Set("ResponseDelayForOrderChangeTimeInMS", "0");
            ConfigReader.Set("directticketingpcc", "6UOL");
            ConfigReader.Set("microservicedbkey", "abc");
            ConfigReader.Set("microserviceresponsetablename", "xyz");
            ConfigReader.Set("InterLineOperatingSegments", "Test");
        }

        [Fact]
        public void HandlerShouldThrowException_WhenMediatorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderChangeEventSubscribeCommandHandler(null, _logger.Object, _timeChangeDisruptionCommand, _cancellationDisruptionCommand, _reshopDisruptionCommand, _serviceBusTopicMessageEventCommandHandler, _orderChangeReshopHoldBookingCommand));
        }

        [Fact]
        public void HandlerShouldThrowException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderChangeEventSubscribeCommandHandler(_mediator, null, _timeChangeDisruptionCommand, _cancellationDisruptionCommand, _reshopDisruptionCommand, _serviceBusTopicMessageEventCommandHandler, _orderChangeReshopHoldBookingCommand));
        }

        [Fact]
        public async Task OrderChangeSuccessScenario_WithValidReq()
        {
            //Arrange
            var req = TestDataProvider.GetTestData<OrderChangeEventProcessCommand>("NDCOrderMgmtMS\\OrderChangeRequestData");

            //Act
            var result = await _handler.Handle(req, CancellationToken.None);

            //Assert
            var item = await _tableStorage.RetrieveItemsAsync(default);
            var orderdoc = Json.Deserialize<OrderDocument>(item.First().ResponseData);

            Assert.Single(orderdoc.DataList.SegmentDetails);
            var seg = orderdoc.DataList.SegmentDetails.First();
            Assert.Equal("LHR", seg.OriginAirportCode);
            Assert.Equal("ATL", seg.DestinationAirportCode);
            Assert.Equal("2026-12-04T10:45:00", seg.ScheduledDepartureTime.LocalDateTime);

            Assert.Equal("9322520182546", orderdoc.OrderItems.First().AccountableDocumentRefs.First().DocumentNumber);

            Assert.Equal(Domain.ChangeTypeEnum.DocumentExchanged, orderdoc.ChangeType);
        }
    }
}
