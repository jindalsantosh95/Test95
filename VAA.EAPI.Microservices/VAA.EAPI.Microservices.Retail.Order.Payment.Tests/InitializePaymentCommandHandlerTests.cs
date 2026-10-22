using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Framework.Tests;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class InitializePaymentCommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<Infrastructure.Services.CPD.InitializePaymentCommandHandler>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly Infrastructure.Services.CPD.InitializePaymentCommandHandler _handler;

        public InitializePaymentCommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<Infrastructure.Services.CPD.InitializePaymentCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new Infrastructure.Services.CPD.InitializePaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Infrastructure.Services.CPD.InitializePaymentCommandHandler(null, _logger.Object, _httpConnector.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Infrastructure.Services.CPD.InitializePaymentCommandHandler(_mediator.Object, null, _httpConnector.Object));
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new Infrastructure.Services.CPD.InitializePaymentCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public async Task InitializePaymentCommandHandler_Handle_ShouldSucceed()
        {
            ConfigReader.Set("CPDAuthUsername", "abc_user");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDInitializeRequestUrl", "https://abc.com");

            var initializeRQ = Xml.Serialize(TestDataProvider.GetXmlTestData<InitializePaymentRequest>("PaymentMS\\InitializeRequest"));
            var initializeRS = Xml.Serialize(TestDataProvider.GetXmlTestData<InitializePaymentResponse>("PaymentMS\\InitializeResponse"));

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(initializeRS, Encoding.UTF8, "text/xml")
                });

            var client = new HttpClient(handlerMock.Object);
            var mockClient = _httpConnector.Setup(_ => _.SendAsync(new HttpRequestMessage(), new CancellationToken()));

            var result = await _handler.Handle(new Infrastructure.Services.CPD.InitializePaymentCommand(){ InitializePaymentXMLRequest = initializeRS}, new System.Threading.CancellationToken());

            Assert.Null(result.Value);
        }

    }
}
