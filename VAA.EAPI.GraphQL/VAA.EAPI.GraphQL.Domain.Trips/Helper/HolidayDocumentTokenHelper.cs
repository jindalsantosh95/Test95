using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using CommonGraphql = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper;

public sealed class HolidayDocumentTokenHelper : BaseIdHelper
{
    private const string BookingIdClaim = "booking-id";
    private const string DocumentIdClaim = "document-id";
    private const string DocumentUrlClaim = "document-url";
    private static readonly TimeSpan ValidationClockSkew = TimeSpan.FromSeconds(30);

    public static Task<string> CreateAsync(string bookingId, string documentId, string documentUrl)
    {
        var claims = new List<Claim>
        {
            new(BookingIdClaim, bookingId),
            new(DocumentIdClaim, documentId),
            new(DocumentUrlClaim, documentUrl)
        };

        return GetTokenString(claims, DateTime.UtcNow.AddMinutes(5), ConfigReader.Get(CommonGraphql.BookingIdJwtKey));
    }

    public static async Task<HolidayDocumentTokenContext> ValidateAsync(string token)
    {
        var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
        var tokenHandler = new JsonWebTokenHandler();
        var validationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = ValidationClockSkew
        });

        if (!validationResult.IsValid)
        {
            throw validationResult.Exception ?? new SecurityTokenValidationException("The document token is invalid.");
        }

        var validatedToken = tokenHandler.ReadJsonWebToken(token);
        var bookingId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == BookingIdClaim)?.Value;
        var documentId = validatedToken.Claims.FirstOrDefault(claim => claim.Type == DocumentIdClaim)?.Value;
        var documentUrl = validatedToken.Claims.FirstOrDefault(claim => claim.Type == DocumentUrlClaim)?.Value;

        if (bookingId.IsNullOrWhiteSpace() || documentId.IsNullOrWhiteSpace() || documentUrl.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("The document token is missing required claims.", nameof(token));
        }

        return new HolidayDocumentTokenContext(bookingId, documentId, documentUrl);
    }
}

public sealed record HolidayDocumentTokenContext(string BookingId, string DocumentId, string DocumentUrl);
