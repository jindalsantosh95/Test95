using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.VHols.Models.Authentication;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.VHols.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class AuthenticationQuery
    {
        private readonly IHttpConnector _connector;
        private readonly ILogger<AuthenticationQuery> _logger;

        public AuthenticationQuery(IHttpConnector connector, ILogger<AuthenticationQuery> logger)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// The method
        /// </summary>
        /// <param name="surname">The Surname</param>
        /// <param name="dob">The DoB</param>
        /// <param name="bookingRef">The Booking Ref</param>
        /// <returns></returns>
        [GraphQLDescription("Create a new session using. This will provide an authorization token, valid for 20 minutes and a refresh token valid for 21 days.")]
        [GraphQLName("vholsLogin")]
        public async Task<AuthenticationResponse> Login(LoginRequest request)
        {
            return await Authenticate(
                ConfigReader.Get(Common.Constants.Environment.VHolsQueryAPILogin),
                Common.Serialisation.Json.Serialize(request, false));
        }

        [GraphQLDescription("Refresh the current session token, using the refresh token. This will provide an authorization token, valid for 20 minutes and a refresh token valid for 21 days.")]
        [GraphQLName("vholsRefresh")]
        public async Task<AuthenticationResponse> Refresh(string refreshToken)
        {
            return await Authenticate(
                ConfigReader.Get(Common.Constants.Environment.VHolsQueryAPIRefresh),
                Common.Serialisation.Json.Serialize(new RefreshRequest { RefreshToken = refreshToken }, false));
        }

        private async Task<AuthenticationResponse> Authenticate(string url, string request)
        {
            var response = await _connector.SendAsync<AuthenticationResponse>(url, new Dictionary<string, string>(), string.Empty, HttpMethod.Post, request);
            // Surface authentication token from the header
            response.Response.AccessToken = response.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "") ?? "";
            return response.Response;

        }
    }
}
