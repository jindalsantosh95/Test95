using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Core.Functions;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;
using VAA.Facade.EnrolmentFacade.Infrastructure.Services;
using VAA.Facade.EnrolmentFacade.Triggers.Http;
using Xunit;
using static VAA.Facade.Test.Framework.TestHelpers;

namespace VAA.Facade.EnrolmentFacade.Tests
{
    public class FacadeEnrolmentControllerTest
    {
        private const string DefaultHttpResponses = "default";

        private readonly VAA.EAPI.Core.Xunit.Http.HttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public FacadeEnrolmentControllerTest()
        {
            Setup.CreateTestEnvironmentSettings();
            _serviceProvider = BuildServiceProvider();
            _httpClientFactory = _serviceProvider.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>();
        }

        [Fact]
        public async Task Run_ShouldThrow_WhenRequestBodyIsEmpty()
        {
            var controller = _serviceProvider.GetRequiredService<FacadeEnrolmentController>();
            var (request, context) = CreateRequest();

            await Assert.ThrowsAnyAsync<Exception>(() => controller.Run(request, context));
        }

        [Fact]
        public async Task TestController_Returns_UnprocessableEntity_When_Enrolment_Service_Returns_422()
        {
            var controller = _serviceProvider.GetRequiredService<FacadeEnrolmentController>();
            string body = TestDataProvider.GetTestData<string>("EnrollmentNotEventData");
            var (request, context) = CreateRequest(body);

            ConfigureEnrolmentEndpoint("https://enrolment.test/business-error");
            var responseCountsBefore = CaptureResponseCounts();

            var result = await controller.Run(request, context);

            Assert.Equal(HttpStatusCode.UnprocessableEntity, result.StatusCode);
            Assert.Contains(GetNewHttpResponses(responseCountsBefore), response =>
                response.RequestMessage?.RequestUri?.AbsoluteUri == "https://enrolment.test/business-error");
        }

        private static ServiceProvider BuildServiceProvider()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddSingleton<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>()
                .AddSingleton<IHttpClientFactory>(sp => sp.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>())
                .AddSingleton<VAA.EAPI.Infrastructure.Interfaces.IHttpConnector, VAA.EAPI.Infrastructure.Services.HttpConnector>()
                .AddSingleton<VAA.EAPI.Infrastructure.Interfaces.IOAuthTokenProvider, VAA.EAPI.Core.Xunit.Azure.OAuthTokenProvider>()
                .AddSingleton<IHttpServiceHandler, HttpServiceHandler>()
                .AddSingleton<IPnrLinkServiceHandler, PnrLinkServiceHandler>()
                .AddSingleton<ILogger<PnrLinkServiceHandler>>(NullLogger<PnrLinkServiceHandler>.Instance)
                .AddSingleton<ILogger<EnrolmentRequestCommandHandler>>(NullLogger<EnrolmentRequestCommandHandler>.Instance)
                .AddSingleton<ILogger<FacadeEnrolmentController>>(NullLogger<FacadeEnrolmentController>.Instance)
                .AddSingleton<IOAuthTokenProvider, OAuthTokenProviderAdapter>()
                .AddTransient<IEnrolmentRequestCommandHandler, EnrolmentRequestCommandHandler>()
                .AddTransient<FacadeEnrolmentController>()
                .BuildServiceProvider();
        }

        private (TestHttpRequestData Request, TestFunctionContext Context) CreateRequest(string body = "")
        {
            var context = new TestFunctionContext(new Dictionary<string, object>(), new TestFunctionDefinition(), _serviceProvider);
            var bodyStream = new MemoryStream(Encoding.ASCII.GetBytes(body));
            var request = new TestHttpRequestData(context, new Uri("https://enrolment.facade.test/customers/enrolment"), bodyStream, "POST", CreateHeaders());

            return (request, context);
        }

        private static Dictionary<string, string> CreateHeaders()
        {
            return new Dictionary<string, string>
            {
                { "channelid", "Delta.com" },
                { "channeluserid", "Test@fly.virgin.com" },
                { "Authorization", "Bearer dummy-token" }
            };
        }

        private static void ConfigureEnrolmentEndpoint(string url)
        {
            System.Environment.SetEnvironmentVariable(Constants.Constants.RetrieveCustomerEndpoint, url, System.EnvironmentVariableTarget.Process);
            System.Environment.SetEnvironmentVariable(Constants.Environment.RetryAttempt, "1", System.EnvironmentVariableTarget.Process);
        }

        private IReadOnlyDictionary<string, int> CaptureResponseCounts()
        {
            return new Dictionary<string, int>
            {
                [DefaultHttpResponses] = _httpClientFactory.GetResponses().Count
            };
        }

        private List<HttpResponseMessage> GetNewHttpResponses(IReadOnlyDictionary<string, int> responseCountsBefore)
        {
            return _httpClientFactory.GetResponses()
                .Skip(responseCountsBefore[DefaultHttpResponses])
                .ToList();
        }
    }
}
