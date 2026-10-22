using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Common.Helper
{
    public abstract class BaseIdHelper
    {
        protected static async Task<JsonWebToken> GetAsJsonWebToken(string id, string jwtKey)
        {
            var tokenHandler = new JsonWebTokenHandler();

            var validatedToken = await tokenHandler.ValidateTokenAsync(id, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            });

            if (validatedToken.IsValid)
                return tokenHandler.ReadJsonWebToken(id);
            else
                throw validatedToken.Exception;
        }

        protected static async Task<string> GetTokenString(List<Claim> claims, DateTime expiry, string jwtKey)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiry,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)), SecurityAlgorithms.HmacSha256Signature)
            });
            return tokenHandler.WriteToken(token);
        }

        protected static async Task<string?> GetClaim(string id, string jwtKey, string claimType)
        {
            return (await GetAsJsonWebToken(id, jwtKey)).Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
        }

        protected static async Task<List<Claim>> MergeClaim(List<Claim> claims, string? existingId, string jwtKey, string claimType, string? claimValue)
        {
            if (claimValue != null)
            {
                claims.Add(new Claim(claimType, claimValue));
            }
            else if (existingId != null)
            {
                var selectedPassengersClaim = await GetClaim(existingId, jwtKey, claimType);
                if (selectedPassengersClaim != null) claims.Add(new Claim(claimType, selectedPassengersClaim));
            }

            return claims;
        }
    }
}
