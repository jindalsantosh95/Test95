using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;
using VAA.EAPI.Web.Common.Infrastructure.Interfaces;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL
{
    public class GraphQLRequestHandler : IRequestHandler<GraphQLRequest, GraphQLResponse>
    {
        protected readonly IHttpConnector _msConnector;
        private readonly ILogger<GraphQLRequestHandler> _logger;

        public GraphQLRequestHandler(IHttpConnector msConnector, ILogger<GraphQLRequestHandler> logger)
        {
            _msConnector = msConnector ?? throw new ArgumentNullException(nameof(msConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<GraphQLResponse> Handle(GraphQLRequest request, CancellationToken cancellationToken)
        {
            var trace = _logger.CreateTrace();

            string graphQLUrl = request.GraphqlUrl ?? ConfigReader.Get(Constants.Environment.GraphQLUrl);

            var httpResponseMessage = await _msConnector.SendAsync(graphQLUrl, request.Headers, string.Empty, HttpMethod.Post, Json.Serialize(request));

            var graphQLResponse = await httpResponseMessage.Content.ReadAsAsync<GraphQLResponse>();

            if (graphQLResponse.Errors.Any())
            {
                _logger.LogError($"Request URI: {httpResponseMessage.RequestMessage.RequestUri}");
                _logger.LogError($"Error: {httpResponseMessage.StatusCode.GetHashCode()} - {httpResponseMessage.ReasonPhrase}");
                _logger.LogError($"Exception occurred at " + await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken));

                throw (GraphQLException)graphQLResponse;
            }

            if (httpResponseMessage.Headers.Any())
                AsyncContext.Current.Context.Items.TryAdd(Constants.Application.BasketIdHeaderContainer, httpResponseMessage.Headers);

            trace.Diagnostics.LogInformation($"GraphQL Request Handler, PqId:{request.PqId}, executed in {{2}}ms");

            return graphQLResponse;
        }
    }
}
