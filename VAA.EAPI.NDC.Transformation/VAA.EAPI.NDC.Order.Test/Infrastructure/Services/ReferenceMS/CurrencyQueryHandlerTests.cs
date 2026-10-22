using Microsoft.Azure.Documents.SystemFunctions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ReferenceMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS;
using Xunit;

namespace VAA.EAPI.NDC.Order.Tests.Infrastructure.Services.ReferenceMS
{
    public class CurrencyQueryHandlerTests
    {
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly ReferenceMSCurrencyQueryHandler _handler;

        public CurrencyQueryHandlerTests()
        {
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new ReferenceMSCurrencyQueryHandler(_httpConnector.Object);
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReferenceMSCurrencyQueryHandler(null));
        }

        [Fact]
        public async Task ReferenceMSCurrencyHandler_ShouldSucceed()
        {
            //Arrange
            ConfigReader.Set("ReferenceMSSubscriptionKey", "1234");
            ConfigReader.Set("ReferenceMSUrl", "http://test.com");

            var request = new ReferenceMSCurrencyQuery() { ReferenceDataType = string.Empty };

            var currencies = new List<Currency>()
            {
                new Currency()
                {
                    CurrencyCode = "GBP",
                    CurrencyNumber = "123"
                }
            };

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(Json.Serialize(currencies))
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(request, new CancellationToken());
            var currencyResult = Json.Deserialize<IEnumerable<Currency>>(result);

            //Assert
            Assert.Equal("123", currencyResult.First().CurrencyNumber);
            Assert.Equal("GBP", currencyResult.First().CurrencyCode);

        }
    }
}
