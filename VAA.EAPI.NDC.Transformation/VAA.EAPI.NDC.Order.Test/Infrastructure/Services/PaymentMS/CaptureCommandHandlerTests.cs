using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.PaymentMS
{
    public class CaptureCommandHandlerTests
    {
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly PaymentMSCaptureCommandHandler _handler;

        public CaptureCommandHandlerTests()
        {
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new PaymentMSCaptureCommandHandler(_httpConnector.Object);
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PaymentMSCaptureCommandHandler(null));
        }

        [Fact]
        public async Task CapturePayment_ShouldSucceed()
        {
            //Arrange
            ConfigReader.Set("PaymentMSSubscriptionKey", "1234");
            ConfigReader.Set("PaymentMSCaptureUrl", "http://test.com");

            var paymentDocument = new PaymentDocument()
            {
                TransactionId = "123456",
                OrderID = "VS932ABCDEF24"
            };

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(Json.Serialize(paymentDocument))
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(new PaymentMSCaptureCommand(), new CancellationToken());

            //Assert
            Assert.Equal("123456", result.PaymentDocument.TransactionId);
            Assert.Equal("VS932ABCDEF24", result.PaymentDocument.OrderID);

        }

        [Fact]
        public async Task CapturePayment_ShouldReturnErrorList()
        {
            //Arrange
            ConfigReader.Set("PaymentMSSubscriptionKey", "1234");
            ConfigReader.Set("PaymentMSCaptureUrl", "http://test.com");

            var errorList = new List<ErrorDetails>()
            {
                new ErrorDetails()
                {
                    Code = "G066",
                    Message = "Code: 400; Message: Element 'product': The attribute 'sku' is required but missing.",
                    DeveloperMessage = "Code: 400; Message: Element 'product': The attribute 'sku' is required but missing. | Error occurred",
                    ErrorSource = "PaymentMS"
                }
            };

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent(Json.Serialize(errorList))
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(new PaymentMSCaptureCommand(), new CancellationToken());

            //Assert
            Assert.Equal("G066", result.Errors.First().Code);
            Assert.Contains("'sku' is required but missing", result.Errors.First().Message);
            Assert.Equal("PaymentMS", result.Errors.First().ErrorSource);

        }
    }
}
