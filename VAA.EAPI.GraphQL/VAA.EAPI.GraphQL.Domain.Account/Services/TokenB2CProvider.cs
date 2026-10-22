using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class TokenB2CProvider<T> : ITokenB2CProvider<T>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IAccountCustomerService _accountCustomerService;
        private readonly ILogger<TokenB2CProvider<T>> _logger;

        public TokenB2CProvider(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector, IAccountCustomerService accountCustomerService, ILogger<TokenB2CProvider<T>> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _accountCustomerService = accountCustomerService ?? throw new ArgumentNullException(nameof(accountCustomerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<T> ValidateTokenAsync(string token)
        {
            var headers = new Dictionary<string, string>
            {
                { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey) },
                { Application.Token, token }
            };

            var identityResponse = (await _httpConnector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.ValidateAuthToken}", headers, await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId)), HttpMethod.Get, string.Empty)).Response;

            if (identityResponse is not string tokenResponse)
            {
                return identityResponse;
            }

            if (typeof(T) != typeof(string))
            {
                throw new NotSupportedException($"{nameof(TokenB2CProvider<T>)} only supports string responses.");
            }

            TokenB2CResponse claim;
            try
            {
                claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize token response from IdentityMS.");
                return identityResponse;
            }

            if (claim == null)
            {
                return identityResponse;
            }

            var hasAllRequiredClaims = !string.IsNullOrWhiteSpace(claim.FcNumber)
                                       && !string.IsNullOrWhiteSpace(claim.MemberStatus);

            if (hasAllRequiredClaims)
            {
                return identityResponse;
            }

            var customerAccount = await GetCustomerProfileByEmailAsync(claim.EmailAddress);
            if (customerAccount == null)
            {
                return identityResponse;
            }

            claim.CustomerId = string.IsNullOrWhiteSpace(claim.CustomerId) ? customerAccount.CustomerId : claim.CustomerId;
            claim.FcNumber = string.IsNullOrWhiteSpace(claim.FcNumber) ? customerAccount.LoyaltyMemberNumber : claim.FcNumber;
            claim.MemberStatus = string.IsNullOrWhiteSpace(claim.MemberStatus) ? customerAccount.Status : claim.MemberStatus;

            return (T)(object)Json.Serialize(claim);
        }

        private async Task<CustomerProfileDetails> GetCustomerProfileByEmailAsync(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                return null;
            }

            try
            {
                var profiles = await _accountCustomerService.GetCustomerDetailsByEmail(new CustomerProfileRequest
                {
                    Email = emailAddress
                });

                return profiles?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve customer profile by email from CustomerMS.");
                return null;
            }
        }
    }
}
