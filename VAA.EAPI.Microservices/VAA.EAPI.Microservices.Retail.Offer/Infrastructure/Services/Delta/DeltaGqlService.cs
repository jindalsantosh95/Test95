using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Graphql;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Delta
{
    public class DeltaGqlService : IDeltaGqlService
    {
        private readonly IGraphQLService _graphqlService;
        private readonly ILogger<IDeltaGqlService> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public DeltaGqlService(IGraphQLService graphqlService, ILogger<IDeltaGqlService> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _graphqlService = graphqlService ?? throw new ArgumentNullException(nameof(graphqlService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<GraphQLResponse> SendAsync(object variables, string gqlQuery, CancellationToken cancellationToken, string? extraLogData = null)
        {
            ArgumentNullException.ThrowIfNull(variables);
            ArgumentNullException.ThrowIfNull(gqlQuery);

            var offerUri = ConfigReader.Get(Constants.Environment.DlAwsOfferGqlUri);
            var token = await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlAwsOfferClientId), ConfigReader.Get(Constants.Environment.DlAwsOfferClientSecret), true);

            _logger.LogInformation($"CorrelationId: {AsyncContext.Current.CorrelationId}, Delta GraphQL Variables:\n{Json.Serialize(variables)} {extraLogData}");

            var headers = new Dictionary<string, string>
            {
                {Constants.Application.ApplicationId, ConfigReader.Get(Constants.Environment.DlApiApplicationId)},
                {Constants.Application.ChannelId, ConfigReader.Get(Constants.Environment.DlApiChannelId)},
                {"transactionId", AsyncContext.Current.CorrelationId.ToString()}
            };
                
            GraphQLRequest graphQLRequest = new()
            {
                GraphQlUri = offerUri,
                Headers = headers,
                Request = Json.Serialize(new ServiceRequest<object>()
                {
                    Variables = variables,
                    Query = gqlQuery,
                }),

                Token = token
            };

            return await _graphqlService.SendAsync(graphQLRequest, cancellationToken);
        }
    }
}