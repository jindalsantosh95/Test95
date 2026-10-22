using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.CPD;
using Xunit;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using Status = VAA.EAPI.Domain.Entities.Common.Status;
namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class InitializeAPICommandHandlerTests
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<InitializePaymentCommandHandler>> _logger;
        private readonly Mock<IHttpConnector> _httpConnector;
        private readonly InitializePaymentCommandHandler _handler;

        public InitializeAPICommandHandlerTests()
        {
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<InitializePaymentCommandHandler>>();
            _httpConnector = new Mock<IHttpConnector>();
            _handler = new InitializePaymentCommandHandler(_mediator.Object, _logger.Object, _httpConnector.Object);
        }

        [Fact]
        public void When_MediatorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new InitializePaymentCommandHandler(null, _logger.Object, _httpConnector.Object));
        }

        [Fact]
        public void When_LoggerObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new InitializePaymentCommandHandler(_mediator.Object, null, _httpConnector.Object));
        }

        [Fact]
        public void When_httpConnectorObjectIsNull_Constructor_ShouldThrowNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new InitializePaymentCommandHandler(_mediator.Object, _logger.Object, null));
        }

        [Fact]
        public async Task InitializePayment_ShouldSucceed()
        {
            //Arrange
            ConfigReader.Set("CPDAuthUsername", "xxx");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDInitializeRequestUrl", "https://abc.com");

            var InitializeRQ = Xml.Serialize(TestDataProvider.GetXmlTestData<InitializePaymentRequest>("PaymentMS\\InitializeRequest"));
            var InitializeRS = Xml.Serialize(TestDataProvider.GetXmlTestData<InitializePaymentResponse>("PaymentMS\\InitializeResponse"));

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(InitializeRS, Encoding.UTF8, "text/xml")
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<StringContent>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(new InitializePaymentCommand() { InitializePaymentXMLRequest = InitializeRQ }, new CancellationToken());

            //Assert
            Assert.Equal(Status.OK, result.Value.Status);
            Assert.True(result.IsSuccess);
            Assert.Equal("80244538", result.Value.Value.Transaction.Id);
        }

        [Fact]
        public async Task InitializePayment_Shouldfailed_With_Invalid_TransactionId()
        {
            //Arrange
            ConfigReader.Set("CPDAuthUsername", "xxx");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDInitializeRequestUrl", "https://abc.com");

            var InitializeRQ = Xml.Serialize(TestDataProvider.GetXmlTestData<InitializePaymentRequest>("PaymentMS\\InitializeRequest"));
            var InitializeRS = Xml.Serialize(TestDataProvider.GetXmlTestData<InitializePaymentResponse>("PaymentMS\\InitializeResponse"));

            var httpResponseMessage = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(InitializeRS, Encoding.UTF8, "text/xml")
            };

            _httpConnector.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<StringContent>(), It.IsAny<bool>())).ReturnsAsync(httpResponseMessage);

            //Act
            var result = await _handler.Handle(new InitializePaymentCommand() { InitializePaymentXMLRequest = InitializeRQ }, new CancellationToken());

            //Assert
            Assert.Equal(Status.BadRequest, result.Value.Status);
            Assert.Equal("4001", result.Value.Value.Session.Status);
            Assert.Equal("80244538", result.Value.Value.Transaction.Id);
        }

        [Fact]
        public void InitializePayment_RequestMapperTest()
        {
            //Arrange
            ConfigReader.Set("CPDAuthUsername", "xxx");
            ConfigReader.Set("CPDAuthPassword", "xyz");
            ConfigReader.Set("CPDInitializeRequestUrl", "https://abc.com");
            ConfigReader.Set("CPDClientId", "123");
            ConfigReader.Set("CPDSaltkey", "abcd");
            ConfigReader.Set("CPDAccount", "456");

            var request = TestDataProvider.GetTestData<Domain.CPD.AuthorizePaymentRequest>("PaymentMS\\PaymentRequest");

            //Act
            var initializeRequest = InitializePaymentMapper.InitializePaymentRequestMapper(request, [new CountryId { CountryCode = "IN", DialingCode = "91", Id = "356" }]);

            //Assert
            Assert.NotNull(initializeRequest);
            Assert.Equal("356", initializeRequest.Initializepayment.Transaction.Amount.CountryId);
            var agencyParams = initializeRequest.Initializepayment.Transaction.AdditionalData.Param;
            Assert.Equal("91281035", agencyParams.First(x => x.Name == "TravelAgencyCode").Text);
            Assert.Equal("Skyscanner", agencyParams.First(x => x.Name == "TravelAgencyName").Text);
            Assert.Equal("agency@example.com", agencyParams.First(x => x.Name == "TravelAgencyEmail").Text);
        }

        [Theory]
        [InlineData(true, true, "12345")]
        [InlineData(true, false, "23456")]
        [InlineData(false, true, "34567")]
        [InlineData(false, false, "45678")]
        public void GetPaymentAccount_ShouldReturnExpectedAccount(bool isUsAgent, bool isMoto, string expectedAccount)
        {
            ConfigReader.Set(Constants.Environment.ARCMotoAccount, "12345");
            ConfigReader.Set(Constants.Environment.ARCEcomAccount, "23456");
            ConfigReader.Set(Constants.Environment.CPDMotoAccount, "34567");
            ConfigReader.Set(Constants.Environment.CPDAccount, "45678");

            var account = PaymentAccountHelper.GetPaymentAccount(isUsAgent, isMoto);

            Assert.Equal(expectedAccount, account);
        }
    }   
}
