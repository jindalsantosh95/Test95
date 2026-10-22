using HotChocolate;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class EntraEnrolFlyingClubMemberMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly EntraEnrolFlyingClubMemberMutation _mutation;

        public EntraEnrolFlyingClubMemberMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            if (httpClientFactoryFixture is null) throw new ArgumentNullException(nameof(httpClientFactoryFixture));

            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.DeDupFCGenUrl, "https://Testresources.virginatlantic.com/dev/microservices/GenerateFcNumber");

            var connector = new HttpConnector(NullLogger<HttpConnector>.Instance, httpClientFactoryFixture.Current);
            var tokenProvider = new TestOAuthTokenProvider();
            var stampingHandler = new NoOpStampingPNRsHandler();

            IEnrolmentProvider enrolmentProvider = new EnrolmentProvider(tokenProvider, connector, stampingHandler);

            _mutation = new EntraEnrolFlyingClubMemberMutation(enrolmentProvider);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenEnrolmentProviderIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EntraEnrolFlyingClubMemberMutation(null!));
        }

        [Fact]
        public async Task EnrolFcMember_ShouldReturnGraphQlError_WhenRequestIsNull()
        {
            Setup.CreateTestEnvironmentSettings();

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EnrolFcMember(null!));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(HttpStatusCode.InternalServerError.ToString(), error.Code);
            Assert.Equal("Error occurred while creating account", error.Extensions!["Exception"]);
            Assert.Contains("request", error.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task EnrolFcMember_ShouldReturnTokenResponse_WhenEnrolmentProviderSucceeds()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-enrol-success");

            var enrolRequest = TestDataProvider.GetTestData<EnrolMemberAccountRequest>("Account//EnrolmentRequest");

            var result = await _mutation.EnrolFcMember(enrolRequest);

            Assert.NotNull(result);
            Assert.Equal("success-continuation-token", result.ContinuationToken);
            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal("openid profile", result.Scope);
            Assert.Equal(3600, result.ExpiresIn);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal("id-token", result.IdToken);
            Assert.Equal("1501033656", result.MemberNumber);
            Assert.Equal(enrolRequest.CustomerDetails?.FirstName, result.FirstName);
            Assert.Equal(enrolRequest.CustomerDetails?.LastName, result.LastName);
        }

        [Fact]
        public async Task EnrolFcMember_ShouldReturnApiErrorDetails_WhenEnrolmentProviderReturnsHttpErrorWithErrorDetails()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-enrol-error");

            var enrolRequest = TestDataProvider.GetTestData<EnrolMemberAccountRequest>("Account//EnrolmentRequest");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EnrolFcMember(enrolRequest));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Account Creation failed", error.Message);
            Assert.Equal("E1", error.Extensions!["code"]);
            Assert.Equal("Invalid request", error.Extensions!["message"]);
            Assert.Equal("IdentityMS", error.Extensions!["source"]);

            var errorsJson = Common.Serialisation.Json.Serialize(error.Extensions!["errors"]);
            var errors = Common.Serialisation.Json.Deserialize<List<ErrorExtension>>(errorsJson);
            var primary = Assert.Single(errors);
            Assert.Equal("E1", primary.Code);
            Assert.Equal("Invalid request", primary.Message);
            Assert.Equal("IdentityMS", primary.Source);
        }

        [Fact]
        public async Task EnrolFcMember_ShouldReturnUnhandledException_WhenEnrolmentProviderReturnsHttpErrorWithoutErrorDetails()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-enrol-empty-errors");

            var enrolRequest = TestDataProvider.GetTestData<EnrolMemberAccountRequest>("Account//EnrolmentRequest");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EnrolFcMember(enrolRequest));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("500", error.Code);
            Assert.Contains("Unhandled Exception in Account Creation", error.Message, StringComparison.Ordinal);
            Assert.Equal("Http connector failed.", error.Extensions!["Exception"]);
        }

        [Fact]
        public async Task EnrolFcMember_ShouldReturnInternalError_WhenCompleteSignUpReturnsEmptyBody()
        {
            Setup.CreateTestEnvironmentSettings();
            ConfigReader.Set(Environment.IdentityMsUrl, "https://Testresources.virginatlantic.com/dev/identitymicroservice/entra-enrol-empty-body");

            var enrolRequest = TestDataProvider.GetTestData<EnrolMemberAccountRequest>("Account//EnrolmentRequest");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => _mutation.EnrolFcMember(enrolRequest));
            var error = Assert.Single(exception.Errors);

            Assert.Equal(HttpStatusCode.InternalServerError.ToString(), error.Code);
            Assert.Equal("Error occurred while creating account", error.Extensions!["Exception"]);
            Assert.Equal("CompleteSignUp returned an empty token response.", error.Message);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("dummy-token");
        }

        private sealed class NoOpStampingPNRsHandler : IStampingPNRsHandler
        {
            public Task StampPNRs(CustomerDTO customerResponse) => Task.CompletedTask;
        }

        private sealed class ErrorExtension
        {
            public string Code { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
        }
    }
}

