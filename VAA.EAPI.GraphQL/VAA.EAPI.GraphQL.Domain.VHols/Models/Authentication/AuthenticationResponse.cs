using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.VHols.Models.Authentication
{
    [GraphQLDescription("Authentication Response: Response to Login and Refresh.")]
    public class AuthenticationResponse
    {
        [GraphQLDescription("The access token for requests to the Vhols booking services. Token is valid for 15 minutes.")]
        public string AccessToken { get; set; }
        [GraphQLDescription("The refresh token to reauthenticate the access token. Token is valid for 3 weeks.")]
        public string RefreshToken { get; set; }
    }
}
