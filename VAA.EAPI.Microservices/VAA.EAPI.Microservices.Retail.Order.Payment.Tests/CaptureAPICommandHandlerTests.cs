using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using Status = VAA.EAPI.Domain.Entities.Common.Status;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class CaptureAPICommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CapturePaymentCommandHandler>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly CapturePaymentCommandHandler _handler;

        public CaptureAPICommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<CapturePaymentCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new CapturePaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CapturePaymentCommandHandler(null, _logger.Object, _httpConnector.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CapturePaymentCommandHandler(_mediator.Object, null, _httpConnector.Object));
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CapturePaymentCommandHandler(_mediator.Object, _logger.Object, null));
        }


        [Fact]
        public async Task CapturePayment_ShouldSucceed()
        {
            //Arrange
            ConfigReader.Set("CPDAuthUsername", "xxx");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDCaptureRequestUrl", "https://abc.com");

            var captureRQ = Xml.Serialize(TestDataProvider.GetXmlTestData<CapturePaymentRequest>("PaymentMS\\CaptureAPIPaymentRequest"));
            var captureRS = Xml.Serialize(TestDataProvider.GetXmlTestData<PaymentResponse>("PaymentMS\\CaptureAPIPaymentResponse"));

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Accepted,
                Content = new StringContent(captureRS, Encoding.UTF8, "text/xml")
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<StringContent>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(new CapturePaymentCommand() { CapturePaymentXMLRequest = captureRQ }, new CancellationToken());

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.True(result.IsSuccess);
            Assert.Equal("100", result.Value.Value.Status.First().Code);
        }

        [Fact]
        public async Task CapturePayment_Shouldfailed_With_Invalid_TransactionId()
        {
            //Arrange
            ConfigReader.Set("CPDAuthUsername", "xxx");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDCaptureRequestUrl", "https://abc.com");

            var captureRQ = Xml.Serialize(TestDataProvider.GetXmlTestData<CapturePaymentRequest>("PaymentMS\\CaptureAPIPaymentRequest"));
            var captureRS = Xml.Serialize(TestDataProvider.GetXmlTestData<PaymentResponse>("PaymentMS\\CaptureAPIPaymentResponse"));

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(captureRS, Encoding.UTF8, "text/xml")
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<StringContent>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(new CapturePaymentCommand() { CapturePaymentXMLRequest = captureRQ }, new CancellationToken());

            //Assert
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.Equal("100", result.Value.Value.Status.First().Code);
            Assert.Equal("200", result.Value.Value.Status.First().Exception.Code.Text);
            Assert.Equal("http", result.Value.Value.Status.First().Exception.Type);
        }

    }
}

