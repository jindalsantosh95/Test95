using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class LoginServiceQuery
    {
        private readonly IOAuthTokenProvider _oauthTokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<LoginServiceQuery> _logger;

        public LoginServiceQuery(IOAuthTokenProvider oauthTokenProvider, IHttpConnector httpConnector, ILogger<LoginServiceQuery> logger)
        {
            _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountLogin")]
        public async Task<string> LoginService(FlyingClubLoginRequest memberLoginRequest)
        {
            _logger.LogInformation("Loyalty Member - GraphQL Login operation");

            if (memberLoginRequest == null)
                throw new ArgumentNullException(nameof(memberLoginRequest));

            var memberStatusValidation = await _httpConnector.SendAsync<string>(ConfigReader.Get(Environment.CustomerMSURLForValidateMemberStatus), GetCustomerHeaders(memberLoginRequest.ChannelId), await GetCustomerToken(), HttpMethod.Post, Json.Serialize(memberLoginRequest));

            var errors = new List<ErrorDetails>();

            if (!string.IsNullOrEmpty(memberStatusValidation.Response))
                errors = Json.Deserialize<List<ErrorDetails>>(memberStatusValidation.Response);

            if (errors != null && errors.Any())
            {
                var builder = ErrorBuilder.New().SetMessage("Login request failed.");
                errors.ForEach(error =>
                {
                    builder.SetExtension("code", error.Code);
                    builder.SetExtension("message", error.Message);
                    builder.SetExtension("source", error.ErrorSource);
                });

                throw new GraphQLException(builder.Build());


            }

            _logger.LogInformation($"Channel id: {memberLoginRequest.ChannelId}");

            var apiResponse = await _httpConnector.SendAsync<string>(ConfigReader.Get(Environment.IdentityMSURLForVerifyMemberInAzureB2C), GetHeaders(memberLoginRequest.ChannelId), await GetToken(), HttpMethod.Post, Json.Serialize(memberLoginRequest));

            if (!string.IsNullOrEmpty(apiResponse.Response) && apiResponse.Response.Contains(Application.ErrorCode))
                errors = Json.Deserialize<List<ErrorDetails>>(apiResponse.Response);

            if (errors != null && errors.Any())
            {
                var builder = ErrorBuilder.New().SetMessage("Login request failed.");
                errors.ForEach(error =>
                {
                    builder.SetExtension("code", error.Code);
                    builder.SetExtension("message", error.Message);
                    builder.SetExtension("source", error.ErrorSource);
                });

                throw new GraphQLException(builder.Build());
            }
            return apiResponse.Response;
        }

        private async Task<string> GenerateMockBearerToken()
        {
            return await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.EnrolmentSignUpAppId));
        }

        private async Task<string> GetToken()
        {
            return await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.EnrolmentSignUpAppId));
        }
        private async Task<string> GetCustomerToken()
        {
            return await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.CustomerAppId));
        }

        private static Dictionary<string, string> GetHeaders(string channelId)
        {
            return new Dictionary<string, string>
            {
                { "ChannelId", string.IsNullOrEmpty(channelId) ? Environment.EnrolmentChannelId : channelId },
                {  Application.OcpKeyHeader , ConfigReader.Get(Environment.EnrolmentMSOcpKey) }
            };
        }
        private static Dictionary<string, string> GetCustomerHeaders(string channelId)
        {
            return new Dictionary<string, string>
            {
                { "ChannelId", string.IsNullOrEmpty(channelId) ? Environment.EnrolmentChannelId : channelId },
                {  Application.OcpKeyHeader , ConfigReader.Get(Environment.CustomerOcpKey) }
            };
        }
    }
}
