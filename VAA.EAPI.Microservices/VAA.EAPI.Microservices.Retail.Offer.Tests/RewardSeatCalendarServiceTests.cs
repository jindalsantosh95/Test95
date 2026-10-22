using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Offer.Domain.RewardSeatAvailability;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Services;
using Xunit;
using XunitHttpClientFactory = VAA.EAPI.Core.Xunit.Http.HttpClientFactory;

namespace VAA.EAPI.Microservices.Retail.Offer.Tests
{

    public sealed class RewardSeatCalendarServiceTests
    {
        private static readonly IReadOnlyDictionary<string, string> Families = new Dictionary<string, string>
        {
            ["REWARD-SEAT-FINDER-ECONOMY"] = "AWARD-ECONOMY",
            ["REWARD-SEAT-FINDER-PREMIUM"] = "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY",
            ["REWARD-SEAT-FINDER-UPPER"] = "AWARD-BUSINESS-FIRST"
        };

        [Theory]
        [InlineData("anchor")]
        [InlineData("start")]
        [InlineData("end")]
        public async Task SearchAsync_RejectsMissingRequiredRequestFieldsBeforeCallingDependencies(string missing)
        {
            var request = Request();
            switch (missing)
            {
                case "anchor": request.AnchorDate = default; break;
                case "start": request.IntervalStart = default; break;
                case "end": request.IntervalEnd = default; break;
            }
            var fares = new FareFamilyStub(Families);
            var ita = new ItaStub((_, _) => throw new InvalidOperationException("ITA should not be called."));

            await Assert.ThrowsAsync<ArgumentException>(() => CreateService(fares, ita)
                .SearchAsync(request, TimeSpan.FromSeconds(5), CancellationToken.None));
            Assert.Empty(fares.RequestedKeys);
            Assert.Equal(0, ita.CallCount);
        }

        [Fact]
        public async Task SearchAsync_RejectsUnsupportedFareFamilyBeforeCallingIta()
        {
            var invalid = new Dictionary<string, string>(Families) { ["REWARD-SEAT-FINDER-PREMIUM"] = "UNSUPPORTED" };
            var ita = new ItaStub((_, _) => throw new InvalidOperationException("ITA should not be called."));

            await Assert.ThrowsAsync<InvalidOperationException>(() => CreateService(
                    new FareFamilyStub(invalid), ita)
                .SearchAsync(Request(), TimeSpan.FromSeconds(5), CancellationToken.None));
            Assert.Equal(0, ita.CallCount);
        }

        [Fact]
        public async Task SearchAsync_RejectsExpiredDeadlineBeforeCallingDependencies()
        {
            var fares = new FareFamilyStub(Families);
            var ita = new ItaStub((_, _) => throw new InvalidOperationException("ITA should not be called."));

            await Assert.ThrowsAsync<TimeoutException>(() => CreateService(fares, ita)
                .SearchAsync(Request(), TimeSpan.Zero, CancellationToken.None));
            Assert.Empty(fares.RequestedKeys);
            Assert.Equal(0, ita.CallCount);
        }

        [Fact]
        public async Task SearchAsync_PropagatesCallerCancellationDuringIta()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set("RewardSeatAvailability:ItaVersion", "161");
            using var cancellation = new CancellationTokenSource();
            var ita = new ItaStub(async (_, token) =>
            {
                cancellation.Cancel();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                throw new InvalidOperationException("Cancelled ITA call should not complete.");
            });

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateService(
                    new FareFamilyStub(Families), ita)
                .SearchAsync(Request(), TimeSpan.FromSeconds(5), cancellation.Token));
            Assert.Equal(1, ita.CallCount);
        }

        [Fact]
        public async Task SearchAsync_RejectsUnconfiguredDefaultMarketBeforeCallingIta()
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();
            var fares = new FareFamilyStub(Families);
            var ita = new ItaStub((_, _) => throw new InvalidOperationException("ITA should not be called."));
            var service = CreateService(fares, ita);
            ConfigReader.Set("RewardSeatAvailability:Markets:GB:Currency", "");

            await Assert.ThrowsAsync<InvalidOperationException>(() => service
                .SearchAsync(Request(), TimeSpan.FromSeconds(5), CancellationToken.None));
            Assert.Equal(0, ita.CallCount);
        }

        [Theory]
        [InlineData("complete-empty", true)]
        [InlineData("business-error", false)]
        [InlineData("malformed", false)]
        public async Task SearchAsync_ExercisesItaHttpAndXmlBoundary(string scenario, bool succeeds)
        {
            OfferTestsCommon.CreateTestEnvironmentSettings();
            ConfigReader.Set(Constants.Environment.ItaServiceUrl, $"https://reward-calendar-ita.test/{scenario}");
            ConfigReader.Set(Constants.Environment.ItaServiceAPIMSubKey, "synthetic-test-key");
            ConfigReader.Set("RewardSeatAvailability:ItaVersion", "161");
            using var provider = new ServiceCollection()
                .AddLogging()
                .AddSingleton<IHttpClientFactory, XunitHttpClientFactory>()
                .AddSingleton<IHttpConnector, HttpConnector>()
                .AddSingleton<IItaService, ItaService>()
                .BuildServiceProvider();
            var fares = new FareFamilyStub(Families);
            var service = CreateService(fares, provider.GetRequiredService<IItaService>());

            if (succeeds)
            {
                var response = await service.SearchAsync(Request(), TimeSpan.FromSeconds(5), CancellationToken.None);
                Assert.True(response.Complete);
                Assert.Empty(response.Candidates);
            }
            else
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.SearchAsync(Request(), TimeSpan.FromSeconds(5), CancellationToken.None));
            }

            var factory = (XunitHttpClientFactory)provider.GetRequiredService<IHttpClientFactory>();
            var recorded = Assert.Single(factory.GetResponses("https://reward-calendar-ita.test"));
            Assert.Equal(HttpStatusCode.OK, recorded.StatusCode);
            Assert.Equal(Families.Keys.OrderBy(value => value), fares.RequestedKeys.OrderBy(value => value));
            Assert.Equal(HttpMethod.Post, recorded.RequestMessage?.Method);
            Assert.Equal($"https://reward-calendar-ita.test/{scenario}", recorded.RequestMessage?.RequestUri?.ToString());
            Assert.Equal("synthetic-test-key", Assert.Single(recorded.RequestMessage!.Headers.GetValues("Ocp-Apim-Subscription-Key")));
            Assert.Equal("text/xml", recorded.RequestMessage.Content?.Headers.ContentType?.MediaType);
        }

        private static RewardSeatCalendarService CreateService(IFareFamily fareFamily, IItaService itaService)
        {
            foreach (var setting in new Dictionary<string, string>
            {
                ["RewardSeatAvailability:MarketPolicyVersion"] = "v1",
                ["RewardSeatAvailability:FareFamilyMappingVersion"] = "v1",
                ["RewardSeatAvailability:Markets:GB:CountryIssued"] = "GBR",
                ["RewardSeatAvailability:Markets:GB:CountrySold"] = "GBR",
                ["RewardSeatAvailability:Markets:GB:Currency"] = "GBP",
                ["RewardSeatAvailability:Markets:GB:SalesCity"] = "LON",
                ["RewardSeatAvailability:BookingCodes:VS:Economy"] = "A",
                ["RewardSeatAvailability:BookingCodes:VS:Premium"] = "P",
                ["RewardSeatAvailability:BookingCodes:VS:Upper"] = "G"
            })
                ConfigReader.Set(setting.Key, setting.Value);

            return new RewardSeatCalendarService(fareFamily, itaService);
        }

        private static RewardSeatCalendarRequest Request() => new()
        {
            Origin = "LHR",
            Destination = "JFK",
            AnchorDate = new DateOnly(2027, 1, 10),
            IntervalStart = new DateOnly(2027, 1, 1),
            IntervalEnd = new DateOnly(2027, 1, 31),
        };

        private sealed class FareFamilyStub(IReadOnlyDictionary<string, string> rows) : IFareFamily
        {
            public IReadOnlyCollection<string> RequestedKeys { get; private set; } = [];
            public Task<string> GetFareFamily(string rowKey) => throw new NotSupportedException();
            public Task<IReadOnlyDictionary<string, string>> GetFareFamilies(
                IReadOnlyCollection<string> rowKeys, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                RequestedKeys = rowKeys.ToArray();
                return Task.FromResult(rows);
            }
        }

        private sealed class ItaStub(Func<search, CancellationToken, Task<result>> getOffers) : IItaService
        {
            public int CallCount { get; private set; }
            public Task<result> GetOffers(search request, CancellationToken cancellationToken)
            {
                CallCount++;
                return getOffers(request, cancellationToken);
            }
            public Task<result> StreamOfferSolutions(search request,
                Func<resultBundledSolutionSummarySolution, CancellationToken, Task> solutionHandler,
                CancellationToken cancellationToken) => throw new NotSupportedException();
        }
    }
}
