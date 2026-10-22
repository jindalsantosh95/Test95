using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Domain.OCI.Tests;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.ContactTracing;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.EmergencyContact;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SavePassport;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.SaveReturnDate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations;
using VAA.EAPI.GraphQL.Domain.PaxOps.Mutations.CheckIn;
using VAA.EAPI.GraphQL.Domain.PaxOps.Queries;
using VAA.EAPI.GraphQL.Domain.PaxOps.Services;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using VAA.EAPI.GraphQL.Infrastructure.Services.Telemetry;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Tests
{
    public class TransactionSummaryLoggingTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly HttpClientFactoryFixture _httpClientFactoryFixture;
        private readonly IServiceProvider _serviceProvider;

        public TransactionSummaryLoggingTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            Setup.CreateTestEnvironmentSettings();
            _httpClientFactoryFixture = httpClientFactoryFixture;

            _serviceProvider = new ServiceCollection()
                .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<ICheckInServiceProvider, CheckInServiceProvider>()
                .AddSingleton<IReferenceServiceProvider, ReferenceServiceProvider>()
                .AddSingleton<IAnalyticsService, AnalyticsService>()
                .AddSingleton<ILogAnalytics, LogAnalytics>()
                .AddSingleton(new TelemetryClient(new TelemetryConfiguration()))
                .AddSingleton<ITelemetryService, TelemetryService>()
                .AddLogging()
                .AddSingleton<CheckInQuery>()
                .AddSingleton<SelectPassengersMutation>()
                .AddSingleton<SavePassportMutation>()
                .AddSingleton<SaveContactTracingMutation>()
                .AddSingleton<EmergencyContactDetailsMutation>()
                .AddSingleton<SaveReturnDateMutation>()
                .BuildServiceProvider();
        }

        [Fact]
        public async Task InitiateCheckIn_LogsTransactionSummary_WhenOnlineCheckInIsUnavailable()
        {
            await _serviceProvider.GetRequiredService<CheckInQuery>().InitiateCheckIn("TSINIT", null, "LHR", "Test", "Passenger", null);

            await AssertTransactionSummaryRequest("TSINIT-travel-party", "TSINIT", "LHR", "initiate-checkin-advisory");
        }

        [Fact]
        public async Task SelectPassengers_LogsTransactionSummary_WhenOnlineCheckInIsUnavailable()
        {
            var checkInId = await CreateCheckInIdAsync("TSSELECT-travel-party", "TSSELECT", "LGW");

            await _serviceProvider.GetRequiredService<SelectPassengersMutation>().SelectPassengers(checkInId, ["transaction-summary-selected-passenger"], null);

            await AssertTransactionSummaryRequest("TSSELECT-travel-party", "TSSELECT", "LGW", "select-passengers-advisory");
        }

        [Fact]
        public async Task SavePassport_LogsTransactionSummary_WhenOnlineCheckInIsUnavailable()
        {
            var checkInId = await CreateCheckInIdAsync("TSPASSPORT-travel-party", "TSPASSPORT", "LHR");

            await _serviceProvider.GetRequiredService<SavePassportMutation>().SavePassport(new SavePassportRequest
            {
                CheckInId = checkInId,
                PassengerId = "transaction-summary-passport",
                Document = new Document
                {
                    DateOfBirth = "1990-01-01",
                    ExpirationDate = "2030-01-01"
                }
            }, null);

            await AssertTransactionSummaryRequest("TSPASSPORT-travel-party", "TSPASSPORT", "LHR", "passport-advisory");
        }

        [Fact]
        public async Task SaveContactTracing_LogsTransactionSummary_WhenOnlineCheckInIsUnavailable()
        {
            var checkInId = await CreateCheckInIdAsync("TSCONTACT-travel-party", "TSCONTACT", "JFK");

            await _serviceProvider.GetRequiredService<SaveContactTracingMutation>().SaveContactTracing(checkInId, false, [new ContactTracing { PassengerId = "transaction-summary-contact" }], null);

            await AssertTransactionSummaryRequest("TSCONTACT-travel-party", "TSCONTACT", "JFK", "contact-tracing-advisory");
        }

        [Fact]
        public async Task SaveEmergencyContactDetails_LogsTransactionSummary_WhenOnlineCheckInIsUnavailable()
        {
            var checkInId = await CreateCheckInIdAsync("TSEMERGENCY-travel-party", "TSEMERGENCY", "ATL");

            await _serviceProvider.GetRequiredService<EmergencyContactDetailsMutation>().SaveEmergencyContactDetails(checkInId, false, [new EmergencyContact { PassengerId = "transaction-summary-emergency" }], null);

            await AssertTransactionSummaryRequest("TSEMERGENCY-travel-party", "TSEMERGENCY", "ATL", "emergency-contact-advisory");
        }

        [Fact]
        public async Task SaveReturnDate_LogsTransactionSummary_WhenOnlineCheckInIsUnavailable()
        {
            var checkInId = await CreateCheckInIdAsync("TSRETURN-travel-party", "TSRETURN", "MAN");

            await _serviceProvider.GetRequiredService<SaveReturnDateMutation>().SaveReturnDate(new SaveReturnDateRequest
            {
                CheckInId = checkInId,
                PassengerId = "transaction-summary-return-date",
                ReturnOn = new LocalDate(2026, 8, 20)
            }, null);

            await AssertTransactionSummaryRequest("TSRETURN-travel-party", "TSRETURN", "MAN", "return-date-advisory");
        }

        private static Task<string> CreateCheckInIdAsync(string travelPartyId, string pnrLocator, string originCode) => CheckInIdHelper.CreateCheckInId(null, "requirements-id", true, travelPartyId, true, pnrLocator, originCode, ["passenger-id"], "Test", "Passenger");

        private async Task AssertTransactionSummaryRequest(string travelPartyId, string recordLocator, string originAirportCode, string transactionError)
        {
            var payload = string.Empty;
            for (var attempt = 0; attempt < 10 && string.IsNullOrEmpty(payload); attempt++)
            {
                var requests = _httpClientFactoryFixture.Current
                    .GetResponses($"{ConfigReader.Get(VAA.EAPI.GraphQL.Common.Constants.Environment.CheckInMSUrl)}/CheckIn/TransactionSummary")
                    .Select(item => item.RequestMessage)
                    .Where(request => request?.RequestUri?.AbsolutePath.EndsWith("/CheckIn/TransactionSummary", StringComparison.OrdinalIgnoreCase) == true);

                foreach (var request in requests)
                {
                    var requestPayload = await request!.Content!.ReadAsStringAsync(CancellationToken.None);
                    if (requestPayload.Contains(transactionError, StringComparison.Ordinal))
                    {
                        payload = requestPayload;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(payload))
                    await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            Assert.NotEmpty(payload);
            Assert.Contains($"\"travelPartyId\":\"{travelPartyId}\"", payload);
            Assert.Contains($"\"recordLocator\":\"{recordLocator}\"", payload);
            Assert.Contains($"\"originAirportCode\":\"{originAirportCode}\"", payload);
            Assert.Contains($"\"transactionError\":\"{transactionError}\"", payload);
        }
    }
}
