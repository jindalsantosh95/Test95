using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Application.OrderChange;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Services;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Triggers.Http;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.NDC.Order.Tests.OrderChange
{
    public class OrderChangeControllerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly ILogger<HttpConnector> _logger;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly HttpConnector _httpConnector;
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<IOrderChangeAmendApisCommand> _orderChangeAmendApisCommand;
        private readonly Mock<IOrderChangeSplitPnrCommand> _orderChangeSplitPnrCommand;
        private readonly OrderChangeController _controller;
        private readonly Mock<FunctionContext> _context;
        private readonly IOrderChangeDisruptionCommand _orderChangeDisruptionCommand;
        private readonly Mock<IOrderChangeCancelHoldBookingCommand> _orderChangeCancelHoldBookingCommand;
        private readonly Mock<ITrackAnalyticsEventCommandHandler> _trackAnalyticsEventCommandHandler;
        private readonly Mock<ILogger<OrderChangeDisruptionCommandHandler>> _disruptionLogger;

        public OrderChangeControllerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _hostEnvironment = new HostingEnvironment { ContentRootPath = Directory.GetCurrentDirectory() };
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _mediator = new Mock<IMediator>();
            _orderChangeAmendApisCommand = new Mock<IOrderChangeAmendApisCommand>();
            _orderChangeSplitPnrCommand = new Mock<IOrderChangeSplitPnrCommand>();
            _disruptionLogger = new Mock<ILogger<OrderChangeDisruptionCommandHandler>>();
            _trackAnalyticsEventCommandHandler = new Mock<ITrackAnalyticsEventCommandHandler>();
            _orderChangeCancelHoldBookingCommand = new Mock<IOrderChangeCancelHoldBookingCommand>();
            _orderChangeDisruptionCommand = new OrderChangeDisruptionCommandHandler(new OrderDataMSQueryHandler(_httpConnector), new ValidationMSCommandHandler(_httpConnector) , new OrderSearchMSCommandHandler(_httpConnector) , new PaymentMSRetrieveQueryHandler(_httpConnector) , new SabreOrderManagementMSCommandHandler(_httpConnector, Microsoft.Extensions.Logging.Abstractions.NullLogger<SabreOrderManagementMSCommandHandler>.Instance) , new PublishOrderDocMSCommandHandler(_httpConnector) , _hostEnvironment , new BrandMSCommandHandler(_httpConnector) , new BrandMSCommandHandlerPTC(_httpConnector), _trackAnalyticsEventCommandHandler.Object, _disruptionLogger.Object);
            _controller = new OrderChangeController(_mediator.Object, _orderChangeAmendApisCommand.Object, _orderChangeSplitPnrCommand.Object, _orderChangeDisruptionCommand, _orderChangeCancelHoldBookingCommand.Object);
            _context = new Mock<FunctionContext>();
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeController(null, _orderChangeAmendApisCommand.Object, _orderChangeSplitPnrCommand.Object, _orderChangeDisruptionCommand, _orderChangeCancelHoldBookingCommand.Object));
        }

        [Fact]
        public void When_OrderChangeAmendApisCommandObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new OrderChangeController(_mediator.Object, null, _orderChangeSplitPnrCommand.Object, _orderChangeDisruptionCommand, _orderChangeCancelHoldBookingCommand.Object));
        }

        [Fact]
        public async Task OrderChange_ControllerShouldReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderChange\\OrderChangeReq", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderChangeEvenExchangeCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.OK });

            //Act
            var result = await _controller.OrderChange(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task OrderChange_ControllerAmendApisShouldReturnSuccess()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderChange\\OrderChangeAmendApisReq", _context, true);
            _orderChangeAmendApisCommand.Setup(x => x.Handle(It.IsAny<IATA_OrderChangeRQ>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>()
            {
                Status = EAPI.Domain.Entities.Common.Status.OK,
                Value = new Domain.OrderViewRS.IATA_OrderViewRS()
                {
                    Items = new object[]
                    {
                        new OrderViewResponseType()
                        {
                            DataLists = new Domain.OrderViewRS.DataListsType()
                            {
                                 PaxList = [new Domain.OrderViewRS.PaxType
                                 {
                                     IdentityDoc = [new Domain.OrderViewRS.IdentityDocType
                                     {
                                           GivenName = new string[] { "SMITH" },
                                           Surname = "DAVID",
                                           IdentityDocTypeCode = "PT",
                                           IdentityDocID = "12345678",
                                           ResidenceCountryCode = "USA"
                                     }]
                                 }]
                            }
                        }
                    }
                }
            });

            //Act
            var result = await _controller.OrderChange(request.Object);
            var sr = new StreamReader(result.Body);
            sr.BaseStream.Position = 0;
            var resultBody = Xml.Deserialize<IATA_OrderViewRS>(sr.ReadToEnd());
            var identityDoc = ((OrderViewResponseType)(resultBody.Items.First())).DataLists.PaxList.First().IdentityDoc;

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.Equal("PT", identityDoc.First().IdentityDocTypeCode);
            Assert.Equal("DAVID", identityDoc.First().Surname);
            Assert.Equal(["SMITH"], identityDoc.First().GivenName);
            Assert.Equal("12345678", identityDoc.First().IdentityDocID);
            Assert.Equal("USA", identityDoc.First().ResidenceCountryCode);
        }

        [Fact]
        public async Task OrderChange_ControllerShouldReturn422()
        {
            //Arrange
            var request = TestDataProvider.GetMockHttpRequest("NDCOrder\\OrderChange\\OrderChangeReq", _context, true);
            _mediator.Setup(x => x.Send(It.IsAny<OrderChangeEvenExchangeCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new EAPI.Domain.Entities.Common.Response<Domain.OrderViewRS.IATA_OrderViewRS>() { Status = EAPI.Domain.Entities.Common.Status.BusinessException });

            //Act
            var result = await _controller.OrderChange(request.Object);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.UnprocessableEntity, result.StatusCode);
        }
    }
}
