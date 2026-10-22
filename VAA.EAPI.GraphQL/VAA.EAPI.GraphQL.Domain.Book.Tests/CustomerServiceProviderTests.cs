using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Core.Functions;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment;
using VAA.EAPI.GraphQL.Domain.Book.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using Xunit;
using static VAA.EAPI.GraphQL.Framework.Tests.TestHelpers;

namespace VAA.EAPI.GraphQL.Domain.Book.Tests
{
    public class CustomerServiceProviderTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly CustomerServiceProvider _customerServiceProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<HttpConnector> _httplogger;
        private readonly ILogger<CustomerServiceProvider> _customerServiceProviderLogger;
        private readonly Mock<IOAuthTokenProvider> _tokenProvider;


        public CustomerServiceProviderTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("EAPIGraph:AppID", "testValue");
            ConfigReader.Set("EAPIGraph:ClientSecret", "testValue");
            ConfigReader.Set("aad:authority", "https://login.microsoftonline.com/");
            ConfigReader.Set("aad:tenantid", "testvalue");

            _customerServiceProviderLogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomerServiceProvider>.Instance;
            _httplogger = Microsoft.Extensions.Logging.Abstractions.NullLogger<HttpConnector>.Instance;
            _httpConnector = new HttpConnector(_httplogger, httpClientFactoryFixture.Current);
            _tokenProvider = new Mock<IOAuthTokenProvider>();

            _customerServiceProvider = new CustomerServiceProvider(_tokenProvider.Object, _httpConnector, _customerServiceProviderLogger);
            Setup.CreateTestEnvironmentSettings();
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "TestUser") });
            var principal = new ClaimsPrincipal(identity);
            var jwtFeature = new JwtPrincipalFeature(principal, new AuthToken { Token = "" });

            var testContext = new TestFunctionContext();
            testContext.Features.Set(jwtFeature);

            AsyncContext.Current = new AsyncContext() { Context = testContext };
        }

        [Fact]
        public async Task ConstructorShouldThrowNullExceptionWhenRequestIsNull_EnrolVAAccount()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => new CustomerServiceProvider(_tokenProvider.Object, _httpConnector, _customerServiceProviderLogger).EnrolVAAccount(null));
        }

        [Fact]
        public async Task EnrolCustomer_ReturnsResponse()
        {
            CustomerEnrolmentRequest enrolmentRequest = new CustomerEnrolmentRequest()
            {
                CustomerDetails = new Customerdetails()
                {
                    FirstName = "TestFirstName",
                    LastName = "TestLastName",
                    DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-30)),
                    MemberType = "Individual",
                    CustomerType = "VirginAccount",
                    Status = "Active",
                    TermsAndConditionsAccepted = true,
                    MediaCode = "WEB",
                    EnrollmentChannel = "WEB",
                    ContactDetails = new Contactdetails()
                    {
                        EmailDetails = [ new Emaildetail()
                        {
                            EmailId = "testemail@mail.com",
                            PreferredFlag = "Y"
                        }],
                        PhoneDetails = [ new Phonedetail()
                        {
                            PhoneNumber = "1234567890",
                            PreferredFlag = "Y",
                            CountryCode = "44"

                        }]
                    }

                }
            };

            var providerResponse = await _customerServiceProvider.EnrolVAAccount(enrolmentRequest);

            Assert.NotNull(providerResponse);
        }

    }
}
