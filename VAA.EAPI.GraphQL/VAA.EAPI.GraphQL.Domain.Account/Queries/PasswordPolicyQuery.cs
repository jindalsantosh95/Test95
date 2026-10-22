using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class PasswordPolicyQuery
    {
        private readonly IOAuthTokenProvider _oauthTokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<PasswordPolicyQuery> _logger;

        public PasswordPolicyQuery(IOAuthTokenProvider oauthTokenProvider, IHttpConnector httpConnector, ILogger<PasswordPolicyQuery> logger)
        {
            _oauthTokenProvider = oauthTokenProvider ?? throw new ArgumentNullException(nameof(oauthTokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountValidatePassword")]
        public async Task<IEnumerable<PasswordPolicy>> ValidatePassword(PasswordValidatorRequest password)
        {
            try
            {
                var token = await _oauthTokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.IdentityMSAppId));
                var headers = new Dictionary<string, string>()
                    {
                        { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.IdentityOcpKey)}
                    };
                var response = await _httpConnector.SendAsync<IEnumerable<PasswordPolicy>>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.ValidatePasswordPolicies}", headers, token, HttpMethod.Post, Json.Serialize(password));

                return response.Response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to password validation service");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in password validation service");
                throw;
            }
        }
    }
}