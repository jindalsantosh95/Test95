using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Validation
{
    public class AuthTokenValidator : IAuthTokenValidator
    {
        private static readonly JwtSecurityTokenHandler JwtSecurityTokenHandler = new();
        private readonly ILogger<AuthTokenValidator> _logger;

        public AuthTokenValidator(ILogger<AuthTokenValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<IEnumerable<Claim>> Verify(string token)
        {
            JwtSecurityToken jwtSecurityToken = JwtSecurityTokenHandler.ReadJwtToken(token);

            if (CheckTokenIsExpired(jwtSecurityToken) || !CheckMemberStatus(jwtSecurityToken).Contains(Constants.Application.ActiveMemberStatus, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError("Authentication token has either expired or member status is not active");
                return Task.FromResult(Enumerable.Empty<Claim>());
            }

            return Task.FromResult(jwtSecurityToken.Claims);
        }

        private static bool CheckTokenIsExpired(JwtSecurityToken token)
        {
            string tokenExp = token.Claims.First(claim => claim.Type.Is("exp")).Value;
            long.TryParse(tokenExp, out var ticks);

            return DateTime.UtcNow.ToUniversalTime() > DateTimeOffset.FromUnixTimeSeconds(ticks).UtcDateTime;
        }

        private static string CheckMemberStatus(JwtSecurityToken token)
        {
            return token.Claims.FirstOrDefault(claim => claim.Type.Is("memberStatus")) == null ? Constants.Application.ActiveMemberStatus : token.Claims.FirstOrDefault(claim => claim.Type.Is("memberStatus")).Value;
        }

    }
}
