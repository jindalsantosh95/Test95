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
    public class RefundAPICommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<RefundPaymentCommandHandler>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly RefundPaymentCommandHandler _handler;

        public RefundAPICommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<RefundPaymentCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new RefundPaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RefundPaymentCommandHandler(null, _logger.Object, _httpConnector.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RefundPaymentCommandHandler(_mediator.Object, null, _httpConnector.Object));
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RefundPaymentCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public async Task RefundPayment_ShouldSucceed()
        {
            //Arrange
            ConfigReader.Set("CPDAuthUsername", "xxx");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDRefundRequestUrl", "https://abc.com");

            var refundRQ = Xml.Serialize(TestDataProvider.GetXmlTestData<RefundPaymentRequest>("PaymentMS\\RefundPaymentRequest"));           
            var refundRS = Xml.Serialize(TestDataProvider.GetXmlTestData<PaymentResponse>("PaymentMS\\RefundPaymentResponse"));
            
            var httpResponseMessage = new HttpResponseMessage()
            {                
                StatusCode = HttpStatusCode.OK,                
                Content = new StringContent(refundRS, Encoding.UTF8, "text/xml")
            };
            
            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<StringContent>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);            

            //Act
            var result = await _handler.Handle(new RefundPaymentCommand() { RefundPaymentXMLRequest = refundRQ }, new CancellationToken());
            
            //Assert
            Assert.Equal(Status.OK,result.Value.Status);
            Assert.Equal("1000", result.Value.Value.Status.First().Code);            
        }

        [Fact]
        public async Task RefundPayment_Shouldfailed_With_Invalid_TransactionId()
        {
            //Arrange
            ConfigReader.Set("CPDAuthUsername", "xxx");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDRefundRequestUrl", "https://abc.com");

            var refundRQ = Xml.Serialize(TestDataProvider.GetXmlTestData<RefundPaymentRequest>("PaymentMS\\RefundPaymentRequest"));
            var refundRS = Xml.Serialize(TestDataProvider.GetXmlTestData<PaymentResponse>("PaymentMS\\RefundPaymentResponse"));

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(refundRS, Encoding.UTF8, "text/xml")
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<StringContent>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(new RefundPaymentCommand() { RefundPaymentXMLRequest = refundRQ }, new CancellationToken());

            //Assert
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.Equal("1000", result.Value.Value.Status.First().Code);
            Assert.Equal("400", result.Value.Value.Status.First().Exception.Code.Text);
            Assert.Equal("http", result.Value.Value.Status.First().Exception.Type);
        }

    }
}
