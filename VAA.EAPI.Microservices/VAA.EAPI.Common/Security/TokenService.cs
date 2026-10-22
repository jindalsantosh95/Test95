using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Common.Security
{
    public class TokenService<T> : ITokenService<T>
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public string GenerateAccessToken(IEnumerable<T> claim) => GenerateAccessToken(claim, Environment.GetEnvironmentVariable(Constants.Environment.IdentityAcquireTokenIssuer), Environment.GetEnvironmentVariable(Constants.Environment.IdentityAcquireTokenExpiryMinutes).Parse<int>());
        public string GenerateAccessToken(IEnumerable<T> claim, string issuer, int expireAfterMinuites)
        {
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable(Constants.Environment.IdentityAcquireTokenSecret)))
            {
                KeyId = Environment.GetEnvironmentVariable(Constants.Environment.IdentityAcquireTokenKeyId)
            };

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(expireAfterMinuites),
                Issuer = issuer,
                Audience = Environment.GetEnvironmentVariable(Constants.Environment.IdentityAcquireTokenAudience),
                SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature),
#warning this is incorrect for storing claims as the name of type will be passed as the value, entry in generated token will appear as { Constants.Application.Claim, typeof(claim).Fullname }
                Claims = new Dictionary<string, object>()
                {
                    { Constants.Application.Claim, claim },
                }
            };

            var token = _tokenHandler.CreateToken(securityTokenDescriptor);

            return _tokenHandler.WriteToken(token);
        }
    }
}
