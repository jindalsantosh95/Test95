using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public class TokenValidatorExtension : ITokenValidatorExtension
    {
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly ILogger<TokenValidatorExtension> _logger;

        public TokenValidatorExtension(ITokenB2CProvider<string> tokenB2CProvider, ILogger<TokenValidatorExtension> logger)
        {
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> ValidateTokenExtractFCNumber(string token)
        {
            if (token.IsNullOrEmpty()) return string.Empty;

            try
            {
                var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(token);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

                return claim?.FcNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Invalid token | token value: {token}");
            }

            return string.Empty;
        }
    }
}
