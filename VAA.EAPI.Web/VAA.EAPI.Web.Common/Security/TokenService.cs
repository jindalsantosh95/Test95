using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Common.Security
{
    public class TokenService<T> : ITokenService<T>
    {
        private readonly JwtSecurityTokenHandler _tokenHandler = new();

        public string GenerateAccessToken(List<T> claim)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ConfigReader.Get(Constants.Environment.IdentityAcquireTokenSecret)))
            {
                KeyId = ConfigReader.Get(Constants.Environment.IdentityAcquireTokenKeyId)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(ConfigReader.Get<int>(Constants.Environment.IdentityAcquireTokenExpiryMinutes)),
                Issuer = ConfigReader.Get(Constants.Environment.IdentityAcquireTokenIssuer),
                Audience = ConfigReader.Get(Constants.Environment.IdentityAcquireTokenAudience),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
                Claims = new Dictionary<string, object>()
                {
                    { Constants.Application.Claim, claim },
                }
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);

            return _tokenHandler.WriteToken(token);
        }
        public string GenerateAccessToken(List<T> claim, string issuer, int expireAfterMinuites)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ConfigReader.Get(Constants.Environment.IdentityAcquireTokenSecret)))
            {
                KeyId = ConfigReader.Get(Constants.Environment.IdentityAcquireTokenKeyId)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(expireAfterMinuites),
                Issuer = issuer,
                Audience = ConfigReader.Get(Constants.Environment.IdentityAcquireTokenAudience),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
                Claims = new Dictionary<string, object>()
                {
                    { Constants.Application.Claim, claim },
                }
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);

            return _tokenHandler.WriteToken(token);
        }

    }
}
