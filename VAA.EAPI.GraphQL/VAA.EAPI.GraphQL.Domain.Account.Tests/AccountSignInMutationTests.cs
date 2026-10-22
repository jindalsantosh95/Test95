using HotChocolate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Mutations;
using VAA.EAPI.GraphQL.Domain.Account.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;

namespace VAA.EAPI.GraphQL.Domain.Account.Tests
{
    [Collection("EnvironmentDependentTests")]
    public sealed class AccountSignInMutationTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public AccountSignInMutationTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();

            _serviceProvider = new ServiceCollection().AddLogging()
                                          .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                          .AddSingleton<IHttpConnector, HttpConnector>()
                                          .AddSingleton<IAccountCustomerService, CustomerService>()
                                          .AddSingleton<IOAuthTokenProvider, TestOAuthTokenProvider>()
                                          .AddSingleton<AccountSignInMutation>()
                                          .AddSingleton<IReferenceDataProvider, ReferenceDataProvider>()
                                          .BuildServiceProvider();


            Setup.CreateTestEnvironmentSettings();
        }


        [Fact]
        public async Task SignInFcMember_ShouldPopulateEmail_WhenEmailIsMissing()
        {
            var mutation = _serviceProvider.GetRequiredService<AccountSignInMutation>();

            var request = new SignInRequest
            {
                Email = string.Empty,
                FlyingClubNumber = "1501282576",
                Password = "P@ssword!"
            };

            var result = await mutation.SignInFcMember(request);

            Assert.Equal("latrinaselena@example.com", request.Email);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SignInFcMember_ShouldPopulateFlyingClubNumber_WhenEmailIsProvided()
        {
            var mutation = _serviceProvider.GetRequiredService<AccountSignInMutation>();

            var request = new SignInRequest
            {
                Email = "latrinaselena@example.com",
                Password = "P@ssword!"
            };

            var result = await mutation.SignInFcMember(request);

            Assert.Equal("1501282576", request.FlyingClubNumber);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task SignInFcMember_ShouldReturnUnauthorized_WhenCustomerLookupByEmailReturnsNull()
        {
            var mutation = _serviceProvider.GetRequiredService<AccountSignInMutation>();

            var request = new SignInRequest
            {
                Email = "latrinaselena@example.com",
                Password = "P@ssword!"
            };

            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices/null");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.SignInFcMember(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Unauthorized", error.Code);
            Assert.Equal(401, error.Extensions["httpStatusCode"]);
            Assert.Equal("Invalid username or password", error.Extensions["Exception"]);
        }

        [Fact]
        public async Task SignInFcMember_ShouldReturnInactiveMemberError_WhenCustomerStatusIsSuspended()
        {
            var mutation = _serviceProvider.GetRequiredService<AccountSignInMutation>();

            var request = new SignInRequest
            {
                Email = "latrinaselena@example.com",
                Password = "P@ssword!"
            };

            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices/suspended");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.SignInFcMember(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Account member SignIn failed", error.Message);
            Assert.Equal("601", error.Extensions!["code"]);
            Assert.Equal("Account did not meet the required status to log in", error.Extensions!["message"]);
        }

        [Fact]
        public async Task SignInFcMember_ShouldReturnUnauthorized_WhenCustomerLookupByFcNumberReturnsWithNullCustomer()
        {
            var mutation = _serviceProvider.GetRequiredService<AccountSignInMutation>();

            var request = new SignInRequest
            {
                Email = string.Empty,
                FlyingClubNumber = "nullcustomer-member",
                Password = "P@ssword!"
            };

            ConfigReader.Set(Common.Constants.Environment.CustomerMsUrl, "https://Testresources.virginatlantic.com/dev/microservices/null");

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.SignInFcMember(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Unauthorized", error.Code);
            Assert.Equal(401, error.Extensions["httpStatusCode"]);
            Assert.Equal("Invalid username or password", error.Extensions["Exception"]);
        }

        [Theory]
        [InlineData("Active")]
        [InlineData("active")]
        [InlineData("ActivePending")]
        [InlineData("Active Pending")]
        [InlineData("ACTIVE\tPENDING")]
        [InlineData("Inactive")]
        public void CheckMemberStatus_ShouldReturnTrue_WhenStatusIsAllowed(string status)
        {
            Assert.True(Helper.CheckMemberStatus(status));
        }

        [Theory]
        [InlineData("Suspended")]
        [InlineData("Expired")]
        [InlineData(null)]
        public void IsActiveStatus_ShouldReturnFalse_WhenStatusIsNotAllowed(string? status)
        {
            Assert.False(Helper.CheckMemberStatus(status));
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        public async Task SignInFcMember_ShouldReturnUnauthorized_WhenCustomerLookupThrowsHttpRequestException(HttpStatusCode statusCode)
        {
            var mutation = new AccountSignInMutation(
                new ThrowingAccountCustomerService(statusCode),
                _serviceProvider.GetRequiredService<IOAuthTokenProvider>(),
                _serviceProvider.GetRequiredService<IHttpConnector>(),
                NullLogger<AccountSignInMutation>.Instance,
                _serviceProvider.GetRequiredService<IReferenceDataProvider>());

            var request = new SignInRequest
            {
                Email = "latrinaselena@example.com",
                Password = "P@ssword!"
            };

            var exception = await Assert.ThrowsAsync<GraphQLException>(() => mutation.SignInFcMember(request));
            var error = Assert.Single(exception.Errors);

            Assert.Equal("Unauthorized", error.Code);
            Assert.Equal(401, error.Extensions["httpStatusCode"]);
            Assert.Equal("Invalid username or password", error.Extensions["Exception"]);
        }

        private sealed class TestOAuthTokenProvider : IOAuthTokenProvider
        {
            public Task<string> AcquireTokenAsync(string resource) => Task.FromResult("abc");
        }

        private sealed class ThrowingAccountCustomerService(HttpStatusCode statusCode) : IAccountCustomerService
        {
            public Task<global::VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS.CustomerDTO> GetCustomer(string customerId, string memberNumber) =>
                throw new NotSupportedException();

            public Task<List<global::VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile.CustomerProfileDetails>> GetCustomerDetailsByEmail(
                global::VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile.CustomerProfileRequest request) =>
                throw new HttpRequestException("Customer lookup failed", null, statusCode);
        }
    }
}
