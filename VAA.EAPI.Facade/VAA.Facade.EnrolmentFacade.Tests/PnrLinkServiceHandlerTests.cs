using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Infrastructure.Services;
using Xunit;
using static VAA.Facade.Test.Framework.TestHelpers;

namespace VAA.Facade.EnrolmentFacade.Tests
{
    public class PnrLinkServiceHandlerTests
    {
        private readonly VAA.EAPI.Core.Xunit.Http.HttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public PnrLinkServiceHandlerTests()
        {
            _serviceProvider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(sp => sp.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>())
                .AddSingleton<PnrLinkServiceHandler>()
                .BuildServiceProvider();

            _httpClientFactory = _serviceProvider.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>();
            ConfigurePnrEndpoint("https://enrolment.test/pnr-link");
        }

        private PnrLinkServiceHandler Handler =>
            _serviceProvider.GetRequiredService<PnrLinkServiceHandler>();

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenLoggerObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PnrLinkServiceHandler(null, _httpClientFactory));
        }

        [Fact]
        public void Constructor_ShouldThrowNullExceptionWhenHttpClientFactorObjectIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
            new PnrLinkServiceHandler(NullLogger<PnrLinkServiceHandler>.Instance, null));
        }

        [Fact]
        public async Task Handler_ShouldThrowErrorforNullRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PnrLinkService>("PnrLinkServiceRequest");
            request = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => Handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handler_ShouldThrowErrorforInvalidUri()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PnrLinkService>("PnrLinkServiceRequest");
            ConfigurePnrEndpoint("https://enrolment.test/pnr-link-not-found");

            //Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => Handler.Handle(request, CancellationToken.None));
        }


        [Fact]
        public async Task Handler_ShouldReturnErrorforMissingRequestAttribute()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PnrLinkService>("PnrLinkServiceRequest");
            request.Customer.DateOfBirth = null;
            ConfigurePnrEndpoint("https://enrolment.test/pnr-link-missing-attribute");

            //Act
            var result = await Handler.Handle(request, CancellationToken.None);


            //Assert
            Assert.Contains("The required input field `dateOfBirth` is missing.", result);
        }

        [Fact]
        public async Task Handler_ShouldReturnErrorforInvalidRequest()
        {
            //Arrange
            var request = TestDataProvider.GetTestData<PnrLinkService>("PnrLinkServiceRequest");
            request.Customer.ContactDetails.EmailDetails.ToList().First().PreferredFlag = "N";
            ConfigurePnrEndpoint("https://enrolment.test/pnr-link-invalid-request");

            //Act
            var result = await Handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.Contains("Unhandled exception (Sequence contains no matching element)", result);
        }

        private static void ConfigurePnrEndpoint(string url)
        {
            Environment.SetEnvironmentVariable(Constants.Constants.PnrLinkMutationEndpoint, url);
        }
    }
}
