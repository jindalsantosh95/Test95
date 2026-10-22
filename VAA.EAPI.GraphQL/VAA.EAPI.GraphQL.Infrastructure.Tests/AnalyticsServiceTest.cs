using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Core.Xunit.Fixtures;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Services;
using VAA.EAPI.GraphQL.Infrastructure.Services.Analytics;
using Xunit;

namespace VAA.EAPI.GraphQL.Infrastructure.Tests
{
    public class AnalyticsServiceTest : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public AnalyticsServiceTest(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("Analytics:Url", "https://tempuri.org/analyticsService/track");
            ConfigReader.Set("Analytics:OcpSubId", Guid.NewGuid().ToString());

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IAnalyticsService, AnalyticsService>()
                                                      .BuildServiceProvider();
        }

        [Fact]
        public async Task AnalyticsService_Track()
        {
            var analyticsService = _serviceProvider.GetService<IAnalyticsService>()!;

            var data = new Dictionary<string, object>()
            {
                { "Unit1", "value" },
                { "Unit2", 1 },
                { "Unit3", false }
            };

            var trackAnalyticsMessage = new TrackAnalyticsMessage(nameof(AnalyticsService_Track), "unit_test", data);

            Assert.Equal("value", trackAnalyticsMessage.Data["Unit1"]!.ToString());
            Assert.Equal(1, trackAnalyticsMessage.Data["Unit2"]!.ToString().Parse<int>());
            Assert.False(trackAnalyticsMessage.Data["Unit3"]!.ToString().Parse<bool>());

            await analyticsService.TrackAnalytics(trackAnalyticsMessage);

        }
    }
}
