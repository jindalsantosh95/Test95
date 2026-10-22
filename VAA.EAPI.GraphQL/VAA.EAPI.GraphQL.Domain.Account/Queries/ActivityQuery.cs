using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ActivityQuery
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<ActivityQuery> _logger;


        public ActivityQuery(ITokenB2CProvider<string> tokenB2CProvider, ILogger<ActivityQuery> logger)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountActivityDetails")]
        [GraphQLDescription("Retrieve Activity Details for Tier points and Virgin points Transactions")]
        public async Task<IEnumerable<ActivityDetail>> ActivityDetails([Service] IActivityQueryProvider _activityQueryProvider, int page, int pageSize)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                return await _activityQueryProvider.GetActivitiesQueryData(claim, page, pageSize);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Activity Details Query failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                {
                    var builder = ErrorBuilder.New().SetMessage("Activity Details Query failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Activity Details Query\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.ActivityError)
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Activity Details Query failed\r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.ActivityError)
                            .Build());
            }
        }
    }
}
