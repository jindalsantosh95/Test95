using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System.Net;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public class RefreshTokenMutation
    {
        private readonly IEntraServicesProvider<SignInResponse> _entraServicesRefreshToken;
        private readonly ILogger<RefreshTokenMutation> _logger;

        public RefreshTokenMutation(IEntraServicesProvider<SignInResponse> entraServicesRefreshToken, ILogger<RefreshTokenMutation> logger)
        {
            _entraServicesRefreshToken = entraServicesRefreshToken ?? throw new ArgumentNullException(nameof(entraServicesRefreshToken));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountRefreshToken")]
        [GraphQLDescription("Generate Refresh Token for Login.")]
        public async Task<SignInResponse> RefreshTokens(RefreshTokenRequest request)
        {
            try
            {
                if (request.Token.IsNullOrEmpty()) throw new ArgumentNullException(nameof(request.Token));

                return await _entraServicesRefreshToken.GenerateRefreshToken(request);
            }

            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception in generating Refresh Token");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);


                if (!errorDetails.IsNullOrEmpty())
                {
                    var builder = ErrorBuilder.New().SetMessage("Generating Refresh Token failed");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    }
                    ;

                    throw new GraphQLException(builder.Build());
                }

                throw RefreshTokenExceptionResponse(HttpStatusCode.InternalServerError.ToString(), $"Error While generating Refresh Token for LogIn\r\n{ex.Message}", ex.Message);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception While generating Refresh Token for LogIn - Exception - {ex.Message}", ex);
                throw RefreshTokenExceptionResponse(HttpStatusCode.InternalServerError.ToString(), "Error While generating Refresh Token for LogIn", ex.Message);
            }
        }

        private static GraphQLException RefreshTokenExceptionResponse(string code, string message, string exception)
        {
            return new GraphQLException(ErrorBuilder.New()
                            .SetMessage(message)
                            .SetExtension("Exception", exception)
                            .SetCode(code)
                            .Build());
        }
    }
}
