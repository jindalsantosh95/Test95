using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Domain;
using VAA.Facade.EnrolmentFacade.Domain.CustomerDetails;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;
using VAA.Facade.EnrolmentFacade.Infrastructure.Services;
using Xunit;

namespace VAA.Facade.EnrolmentFacade.Tests
{
    public class EnrolmentRequestCommandHandlerTest
    {
        private const string PnrLinkEndpoint = "https://enrolment.test/pnr-link";

        private readonly VAA.EAPI.Core.Xunit.Http.HttpClientFactory _httpClientFactory;
        private readonly IServiceProvider _serviceProvider;

        public EnrolmentRequestCommandHandlerTest()
        {
            Setup.CreateTestEnvironmentSettings();
            _serviceProvider = BuildServiceProvider();
            _httpClientFactory = _serviceProvider.GetRequiredService<VAA.EAPI.Core.Xunit.Http.HttpClientFactory>();
        }

        [Fact]
        public async Task Handle_Should_Return_Success_Response()
        {
            var request = BuildRequest(new List<Preferences>
            {
                new Preferences { PreferenceType = Preferences.PreferenceTypeEnum.PreferredPartnerFFP, PreferenceValue = "DL" }
            });
            ConfigureEnrolmentEndpoint("https://enrolment.test/success");

            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task Handle_Should_Return_BusinessException_When_422()
        {
            var request = BuildRequest(new List<Preferences>
            {
                new Preferences { PreferenceType = Preferences.PreferenceTypeEnum.PreferredPartnerFFP, PreferenceValue = "DL" }
            });
            ConfigureEnrolmentEndpoint("https://enrolment.test/business-error");

            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.BusinessException, result.Value.Status);
            Assert.NotNull(result.Value.Errors);
        }

        [Fact]
        public async Task Handle_Should_Return_Error_When_NonSuccess()
        {
            var request = BuildRequest(new List<Preferences>
            {
                new Preferences { PreferenceType = Preferences.PreferenceTypeEnum.PreferredPartnerFFP, PreferenceValue = "DL" }
            });
            ConfigureEnrolmentEndpoint("https://enrolment.test/error");

            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.Equal(Status.Error, result.Value.Status);
        }

        [Fact]
        public async Task Handle_Should_Invoke_PnrLinkService_When_No_PreferredPartner()
        {
            var request = BuildRequest(new List<Preferences>
            {
                new Preferences { PreferenceType = Preferences.PreferenceTypeEnum.VAAMarketingPermissions, PreferenceValue = "Y" }
            });
            ConfigureEnrolmentEndpoint("https://enrolment.test/success");
            ConfigurePnrEndpoint();
            var responseCountsBefore = CaptureResponseCounts();

            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result.Value);
            Assert.Contains(GetNewHttpResponses(responseCountsBefore), response =>
                response.RequestMessage?.RequestUri?.AbsoluteUri == PnrLinkEndpoint);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Handle_Should_Invoke_PnrLinkService_When_PreferredPartnerValueIsEmptyOrNull(string preferenceValue)
        {
            var request = BuildRequest(new List<Preferences>
             {
                 new Preferences { PreferenceType = Preferences.PreferenceTypeEnum.PreferredPartnerFFP, PreferenceValue = preferenceValue }
             });
            ConfigureEnrolmentEndpoint("https://enrolment.test/success");
            ConfigurePnrEndpoint();
            var responseCountsBefore = CaptureResponseCounts();

            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result.Value);
            Assert.Contains(GetNewHttpResponses(responseCountsBefore), response =>
                response.RequestMessage?.RequestUri?.AbsoluteUri == PnrLinkEndpoint);
        }

        [Fact]
        public async Task Handle_Should_Not_Invoke_PnrLinkService_When_PreferredPartnerExists()
        {
            var request = BuildRequest(new List<Preferences>
            {
                new Preferences { PreferenceType = Preferences.PreferenceTypeEnum.PreferredPartnerFFP, PreferenceValue = "DL" }
            });
            ConfigureEnrolmentEndpoint("https://enrolment.test/success");
            ConfigurePnrEndpoint();
            var responseCountsBefore = CaptureResponseCounts();

            var result = await Handler.Handle(request, CancellationToken.None);

            Assert.NotNull(result.Value);
            Assert.DoesNotContain(GetNewHttpResponses(responseCountsBefore), response =>
                response.RequestMessage?.RequestUri?.AbsoluteUri == PnrLinkEndpoint);
        }

        private EnrolmentRequestCommandHandler Handler =>
            _serviceProvider.GetRequiredService<EnrolmentRequestCommandHandler>();

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
                .AddSingleton<IOAuthTokenProvider, OAuthTokenProviderAdapter>()
                .AddTransient<EnrolmentRequestCommandHandler>()
                .BuildServiceProvider();
        }

        private static void ConfigureEnrolmentEndpoint(string url)
        {
            System.Environment.SetEnvironmentVariable(Constants.Constants.RetrieveCustomerEndpoint, url, System.EnvironmentVariableTarget.Process);
            ConfigureRetryAttempts("1");
        }

        private static void ConfigureRetryAttempts(string retryAttempts)
        {
            System.Environment.SetEnvironmentVariable(Constants.Environment.RetryAttempt, retryAttempts, System.EnvironmentVariableTarget.Process);
        }

        private static void ConfigurePnrEndpoint()
        {
            System.Environment.SetEnvironmentVariable(Constants.Constants.PnrLinkMutationEndpoint, PnrLinkEndpoint, System.EnvironmentVariableTarget.Process);
        }

        private IReadOnlyDictionary<string, int> CaptureResponseCounts()
        {
            return new Dictionary<string, int>
            {
                ["http://tempuri.org/"] = _httpClientFactory.GetResponses().Count
            };
        }

        private List<HttpResponseMessage> GetNewHttpResponses(IReadOnlyDictionary<string, int> responseCountsBefore)
        {
            return _httpClientFactory.GetResponses()
                .Skip(responseCountsBefore["http://tempuri.org/"])
                .ToList();
        }

        private static EnrolmentRequestCommand BuildRequest(IEnumerable<Preferences> prefs)
        {
            return new EnrolmentRequestCommand
            {
                Token = "dummy-token",
                ChannelId = "web",
                ChannelUserId = "user1",
                EnrolmentRequest = new EnrolmentRequest
                {
                    CustomerDetails = new Customer
                    {
                        Preferences = prefs?.ToList()
                    }
                }
            };
        }
    }
}
