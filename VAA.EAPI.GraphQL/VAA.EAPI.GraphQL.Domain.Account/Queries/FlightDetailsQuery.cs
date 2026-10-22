using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class FlightDetailsQuery
    {
        private readonly ILogger<FlightDetailsQuery> _logger;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;

        public FlightDetailsQuery(ILogger<FlightDetailsQuery> logger, ITokenB2CProvider<string> tokenB2CProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
        }

        [GraphQLName("accountFlightDetails")]
        [GraphQLDescription("Retrieve Flight details")]
        public async Task<FlightDetailsResponse> GetFlightDetails([Service] IFlightQueryProvider _flightQueryProvider, int page, int pageSize)
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (jwtPrincipalFeature?.AccessToken.Token.IsNullOrWhiteSpace() ?? true)
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                return await _flightQueryProvider.GetFlightsQueryData(claim?.FcNumber, page, pageSize);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Flight Details Query failed");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Any())
                {
                    var builder = ErrorBuilder.New().SetMessage("Flight Details Query  failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage($"Unhandled Exception in Flight Details query\r\n{ex.Message}")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.FlightsError)
                            .Build());
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Unhandled Exception in Flight Details query\r\n{ex.Message}");
                throw new GraphQLException(ErrorBuilder.New()
                            .SetMessage("Miscellaneous technical error")
                            .SetExtension("Exception", ex.Message)
                            .SetCode(Constants.Application.FlightsError)
                            .Build());
            }
        }
    }
}
