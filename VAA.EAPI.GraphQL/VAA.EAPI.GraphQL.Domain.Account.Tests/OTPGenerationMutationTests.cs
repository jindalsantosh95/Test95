using HotChocolate;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class OTPGenerationMutationTests
    {
        private readonly Mock<ILogger<OTPGenerationMutation>> _mockLogger;
        private readonly Mock<IOTPMutationProvider<string>> _otpGenerationMutationProvider;
        private readonly OTPGenerationMutation _mutation;

        public OTPGenerationMutationTests()
        {
            _otpGenerationMutationProvider = new Mock<IOTPMutationProvider<string>>();
            _mockLogger = new Mock<ILogger<OTPGenerationMutation>>();

            _mutation = new OTPGenerationMutation(_otpGenerationMutationProvider.Object, _mockLogger.Object);
            ConfigReader.Set("MyAccount:EnableMockResponseForGenerateOTP", "false");

        }


        [Fact]
        public void Constructor_ShouldThrowNullException_WhentokeProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OTPGenerationMutation(null, _mockLogger.Object));

        }


        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OTPGenerationMutation(_otpGenerationMutationProvider.Object, null));
        }


        [Fact]
        public async Task Mutation_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            var request = "Test@fly.virgin.com";

            _otpGenerationMutationProvider.Setup(x => x.GenerateOtp(It.IsAny<string>())).ReturnsAsync("OTP generated successfully");

            //Act
            var result = await _mutation.generateOTP(request);

            //Assert
            Assert.Equal("OTP generated successfully", result);
        }


        [Fact]
        public async Task GenerateOTP_ShouldReturnException_FailureFromAPI()
        {
            //Arrange
            var request = "Test@fly.virgin.com";

            _otpGenerationMutationProvider.Setup(x => x.GenerateOtp(It.IsAny<string>())).ThrowsAsync(new Exception(string.Empty));

            //Assert
            await Assert.ThrowsAsync<Exception>(() => _mutation.generateOTP(request));
        }

        [Fact]
        public async Task GenerateOTP_ShouldReturnGRaphQLException_ForOTPGenerationFail()
        {
            //Arrange
            var request = "Test@fly@.virgin.com";
            var content = new StringContent(JsonConvert.DeserializeObject<JToken>(TestDataProvider.GetTestData<string>("Account//OTPGenerationFailErrorResponse")).ToString(Formatting.None), Encoding.UTF8, "application/json");
            var apiResponse = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.UnprocessableEntity, Content = content };

            _otpGenerationMutationProvider.Setup(x => x.GenerateOtp(It.IsAny<string>())).ThrowsAsync(new HttpRequestException("HttpConnector failed.", new HttpResponseException(apiResponse), System.Net.HttpStatusCode.InternalServerError));

            //Assert
            await Assert.ThrowsAsync<GraphQLException>(() => _mutation.generateOTP(request));
        }

    }
}
