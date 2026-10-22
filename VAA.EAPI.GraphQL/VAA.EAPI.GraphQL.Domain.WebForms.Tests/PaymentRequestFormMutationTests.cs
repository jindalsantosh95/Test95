using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Tests
{
    public class PaymentRequestFormMutationTests
    {
        private readonly Mock<ILogger<PaymentRequestFormMutation>> _mockLogger;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IHttpConnector> _connector;

        public PaymentRequestFormMutationTests()
        {
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _mockLogger = new Mock<ILogger<PaymentRequestFormMutation>>();
            _connector = new Mock<IHttpConnector>();

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task UpdatePayment_ShouldReturnSuccess_WhenGoodPaymentRequestSupplied()
        {
            //Arrange
            var payment = TestDataProvider.GetTestData<Payment>("Webform\\PaymentRequest");

            //Act
            var response = await new PaymentRequestFormMutation(_mockLogger.Object, _tokenProvider.Object, _connector.Object).UpdatePayment("UpdatePayment", payment);


            //Assert
            Assert.Equal("Success", response.Status.ToString());
        }
    }
}