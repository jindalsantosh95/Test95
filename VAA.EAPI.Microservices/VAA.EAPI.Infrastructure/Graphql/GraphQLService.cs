using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using VAA.EAPI.Common;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Infrastructure.Graphql
{
    public class GraphQLService : IGraphQLService
    {
        protected readonly IHttpConnector _httpConnector;
        private readonly ILogger<GraphQLService> _logger;

        public GraphQLService(IHttpConnector httpConnector, ILogger<GraphQLService> logger)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GraphQLResponse> SendAsync(GraphQLRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var httpResponseMessage = await _httpConnector.SendAsync(request.GraphQlUri, request.Headers, request.Token, HttpMethod.Post, new StringContent(request.Request, Encoding.UTF8, "application/json"));

                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.GatewayTimeout || httpResponseMessage.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                {
                    _logger.LogError($"Correlation id:{AsyncContext.Current.CorrelationId} | Timeout calling GraphQL");

                    return new GraphQLResponse
                    {
                        Errors =
                        [
                            new GraphQLError
                            {
                                Message = httpResponseMessage.ReasonPhrase
                            }
                        ]
                    };
                }
                else
                {
                    var graphqlResponse = await httpResponseMessage.Content.ReadAsAsync<GraphQLResponse>();
                    if (graphqlResponse?.Errors?.Any() == true)
                    {
                        try
                        {
                            // Throw to generate a stack trace
                            throw new GraphQLException($"Errors in GraphQL response: {string.Join(", ", graphqlResponse.Errors?.Select(e => e.Extensions?.Errors?.Message ?? e.Message))}");
                        }
                        catch (GraphQLException ex)
                        {
                            _logger.LogError(ex, string.Join(", ", graphqlResponse.Errors?.Select(e => e.Extensions?.Errors?.DeveloperMessage)));
                        }
                    }
                    return graphqlResponse;
                }
            }
            catch (HttpRequestException ex)
            {
                string errorMessage;
                if (ex.InnerException == null)
                    errorMessage = $"Exception Message: {ex.Message}";
                if (ex.InnerException is HttpResponseException)
                    errorMessage = $"Response: {((HttpResponseException)ex.InnerException).Response}";
                else
                    errorMessage = $"Inner Exception Message: {ex.InnerException.Message}";

                _logger.LogError(ex, errorMessage);

                return new GraphQLResponse
                {
                    Errors =
                    [
                        new GraphQLError
                        {
                            Message = $"Correlation id:{AsyncContext.Current.CorrelationId} request terminated while calling GraphQL. {errorMessage}"
                        }
                    ]
                };
            }
        }
    }
}