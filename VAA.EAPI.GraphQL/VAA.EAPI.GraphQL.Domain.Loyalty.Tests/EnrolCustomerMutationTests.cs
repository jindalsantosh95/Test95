using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public class EnrolCustomerMutationTests
    {
        private readonly Mock<ILogger<EnrolCustomerMutation>> _mockLogger;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;
        private readonly Mock<IHttpConnector> _connector;
        private readonly EnrolCustomerMutation _mutation;

        public EnrolCustomerMutationTests()
        {
            _tokenProvider = new Mock<IOAuthTokenProvider>();
            _mockLogger = new Mock<ILogger<EnrolCustomerMutation>>();
            _connector = new Mock<IHttpConnector>();
            _mutation = new EnrolCustomerMutation(_mockLogger.Object, _tokenProvider.Object, _connector.Object);

            Setup.CreateTestEnvironmentSettings();
        }

        [Fact]
        public async Task EnrolCustomer_ShouldReturnNull_SuccessFromAPI()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<CreateCustomerRequest>("EnrolCustomer//EnrolCustomerRequest");

            //Act
            var result = await _mutation.EnrolCustomer(request);

            //Assert
            Assert.Equal("Ok", result);
        }

        [Fact]
        public async Task EnrolCustomer_ShouldReturnException_FailureFromAPIAsync()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<CreateCustomerRequest>("EnrolCustomer//EnrolCustomerRequest");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity };

            _connector.Setup(x => x.SendAsync<EAPIResponse>(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _mutation.EnrolCustomer(request));

        }
    }
}