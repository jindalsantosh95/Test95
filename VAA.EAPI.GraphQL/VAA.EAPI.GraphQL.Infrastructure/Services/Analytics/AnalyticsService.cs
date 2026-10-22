using global::VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Infrastructure.Services.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        protected readonly IHttpConnector _connector;
        protected readonly ILogger<AnalyticsService> _logger;
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.AanalyticsOcpSubId) } };

        private readonly string _analyticsUrl;

        public AnalyticsService(ILogger<AnalyticsService> logger, IHttpConnector connector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _analyticsUrl = ConfigReader.Get(Common.Constants.Environment.AnalyticsMSUrl) ?? throw new InvalidOperationException("AnalyticsMSUrl not found");
        }

        public async Task TrackAnalytics(TrackAnalyticsMessage trackAnalyticsMessage)
        {
            if (trackAnalyticsMessage is null) return;

            try
            {
                var httpResponseMessage = await _connector.SendAsync(_analyticsUrl, _headers, default, HttpMethod.Post, Json.Serialize(trackAnalyticsMessage), true);

                var response = await httpResponseMessage.Content.ReadAsStringAsync();

                if (!response.Parse<bool>()) 
                    throw new InvalidOperationException("Requested was accepted but analytics failed to process the request.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Correlation Id - {AsyncContext.Current.CorrelationId} - Analytics HTTP request failed with message - {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Correlation Id - {AsyncContext.Current.CorrelationId} - Analytics failed with an unexpected error - {ex.Message}", ex);
            }
        }
    }
}
