using System.Security.Claims;
using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Helper
{
    public class TripsBookingIdHelper : BaseIdHelper
    {
        public static async Task<string> CreateBookingId(string? existingBookingId, string? msBookingId, string? givenName, string? surname, string? pnrLocator)
        {
            var jwtKey = ConfigReader.Get(Common.Constants.Environment.BookingIdJwtKey);

            var claims = new List<Claim>();

            claims = await MergeClaim(claims, existingBookingId, jwtKey, "msBookingId", msBookingId);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "givenName", givenName);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "surName", surname);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "pnrLocator", pnrLocator);

            return await GetTokenString(claims, DateTime.UtcNow.AddDays(1), jwtKey);
        }
    }
}