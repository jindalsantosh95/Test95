using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    public class OtpValidationMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IOTPMutationProvider<string> _mockotpMutationProvider;
        private readonly ICustomerMutationProvider<TierDetail> _customerTierProvider;
        private readonly OTPValidationMutation _mutation;
        private readonly ILogger<HttpConnector> _logger;
        private readonly HttpConnector _httpConnector;
        private readonly ILogger<OTPValidationMutation> _mockLogger;

        public OtpValidationMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_logger, httpClientFactoryFixture.Current);
            _tokenProvider = new TestOAuthTokenProvider();
            _mockLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<OTPValidationMutation>.Instance;
            _customerTierProvider = new CustomerMutationProvider<TierDetail>(_tokenProvider, _httpConnector);
            _mockotpMutationProvider = new OTPMutationProvider<string>(_tokenProvider, _httpConnector);
            _mutation = new OTPValidationMutation(_mockLogger, _mockotpMutationProvider, _customerTierProvider);

            Setup.CreateTestEnvironmentSettings();
        }


        [Fact]
        public void Constructor_ShouldThrowNullException_WhenProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OTPValidationMutation(_mockLogger, null, _customerTierProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OTPValidationMutation(null, _mockotpMutationProvider, _customerTierProvider));
        }

        [Fact]
        public void Constructor_ShouldThrowNullException_WhenCustomerTierProviderObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new OTPValidationMutation(_mockLogger, _mockotpMutationProvider, null));
        }

        [Fact]
        public async Task Mutation_ShouldSucceed_WithGoodRequest()
        {
            //Arrange
            string email = "test.test@fly.test.com";
            int otp = 984923;

            //Act
            var result = await _mutation.validateOTP(email, otp);

            //Assert
            Assert.NotNull(result.EnrolmentToken);
            Assert.False(result.IsEnrolledMember);
            Assert.IsType<OtpValidationResponse>(result);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }

    }
}
