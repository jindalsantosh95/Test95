using HotChocolate;
using HotChocolate.Resolvers;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class PaymentForwardedIpTests
    {
        [Theory]
        [InlineData(" 203.0.113.5 , 198.51.100.7 ", "192.0.2.9")]
        [InlineData(" , ", " 203.0.113.5 , 198.51.100.7 ")]
        public async Task PaymentCheck_ForwardsFirstNonEmptyTrimmedAddress(string firstHeader, string secondHeader)
        {
            Setup.CreateTestEnvironmentSettings();
            using var services = CreateServices();
            var request = await AsyncContext.Current.Context.GetHttpRequestDataAsync();
            request!.Headers.Remove("X-Forwarded-For");
            request.Headers.Add("X-Forwarded-For", new[] { firstHeader, secondHeader });
            var resolver = DispatchProxy.Create<IResolverContext, ErrorRecordingResolver>();

            var result = await services.GetRequiredService<PaymentEligibilityCheckMutation>()
                .PaymentCheck(await CreateRequest(), resolver);

            Assert.NotNull(result);
            Assert.True(result.PaymentPending);
            Assert.Empty(((ErrorRecordingResolver)(object)resolver).Errors);
            var response = Assert.Single(PaymentResponses(services));
            var body = JObject.Parse(await response.RequestMessage!.Content!.ReadAsStringAsync());
            Assert.Equal("203.0.113.5", (string?)body["pos"]?["ipAddress"]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" , , ")]
        public async Task PaymentCheck_ReportsControlledFailureWithoutCallingPayment_WhenNoAddress(string? header)
        {
            Setup.CreateTestEnvironmentSettings();
            using var services = CreateServices();
            var request = await AsyncContext.Current.Context.GetHttpRequestDataAsync();
            request!.Headers.Remove("X-Forwarded-For");
            if (header is not null)
                request.Headers.Add("X-Forwarded-For", header);
            var resolver = DispatchProxy.Create<IResolverContext, ErrorRecordingResolver>();

            var result = await services.GetRequiredService<PaymentEligibilityCheckMutation>()
                .PaymentCheck(await CreateRequest(), resolver);

            Assert.Null(result);
            var error = Assert.Single(((ErrorRecordingResolver)(object)resolver).Errors);
            Assert.Equal(Common.Constants.Application.BaggageCheckInFailDefaultMessage, error.Message);
            Assert.Equal("500", error.Extensions!["code"]);
            Assert.Empty(PaymentResponses(services));
        }

        private static ServiceProvider CreateServices() => new ServiceCollection()
            .AddSingleton<IHttpClientFactory, Core.Xunit.Http.HttpClientFactory>()
            .AddSingleton<IHttpConnector, HttpConnector>()
            .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
            .AddSingleton<IBasketServiceProvider, BasketServiceProvider>()
            .AddSingleton<IBaggageServiceProvider, BaggageServiceProvider>()
            .AddSingleton<IReferenceServiceProvider, ReferenceServiceProvider>()
            .AddSingleton<IOAuthTokenProvider, OAuthTokenProvider_AAD>()
            .AddSingleton<IPaymentServiceProvider, PaymentServiceProvider>()
            .AddSingleton<ITokenB2CProvider<string>, TokenB2CProvider<string>>()
            .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
            .AddSingleton<ITelemetryService, TelemetryService>()
            .AddSingleton<JwtSecurityTokenHandler>()
            .AddLogging()
            .AddSingleton<PaymentEligibilityCheckMutation>()
            .BuildServiceProvider();

        private static IEnumerable<HttpResponseMessage> PaymentResponses(IServiceProvider services) =>
            ((Core.Xunit.Http.HttpClientFactory)services.GetRequiredService<IHttpClientFactory>())
                .GetResponses("https://Testresources.virginatlantic.com")
                .Where(response => response.RequestMessage!.RequestUri!.AbsolutePath.EndsWith("/paymentReferenceId"));

        private static async Task<PaymentCheckRequest> CreateRequest() => new()
        {
            CheckInId = await CheckInIdHelper.CreateCheckInId(null, "FakeRequirementsId", true,
                "TEST02-Id-SSCI_payment", true, "FAKPNR", "LHR",
                ["01.01|49f4da79-c207-4c46-a3a8-833e140cfde9|9999225479295|2387bb6a-b781-48a9-acf2-1def8b0475ea"],
                "Test", "Passenger", "387bf1d4-6aa8-4126-9548-1b882991ef91", true)
        };

        // Only GraphQL error reporting is needed; HTTP dependencies use the real mock-file harness.
        public class ErrorRecordingResolver : DispatchProxy
        {
            public List<IError> Errors { get; } = [];

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod?.Name == nameof(IResolverContext.ReportError) && args?[0] is IError error)
                {
                    Errors.Add(error);
                    return null;
                }

                throw new NotSupportedException(targetMethod?.Name);
            }
        }
    }
}
