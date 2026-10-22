using VAA.EAPI.Common.Helpers;
using Environment = VAA.EAPI.Microservices.Retail.Trip.Constant.Environment;

namespace VAA.EAPI.Microservices.Retail.Trip.Helper
{
    public sealed class BookingIdHelper : BaseIdHelper
    {
        private static string JwtKey => ConfigReader.Get(Environment.BookingIdJwtKey);

        private static Task<string?> GetClaim(string bookingId, string claimType) => GetClaim(bookingId, JwtKey, claimType);

        public static Task<string?> GetBookingId(string bookingId) => GetClaim(bookingId, "msBookingId");

        public static Task<string?> GetGivenName(string bookingId) => GetClaim(bookingId, "givenName");

        public static Task<string?> GetPnrLocator(string bookingId) => GetClaim(bookingId, "pnrLocator");

        public static Task<string?> GetSurName(string bookingId) => GetClaim(bookingId, "surName");

        public static Task<string?> GetBasketId(string bookingId) => GetClaim(bookingId, "basketId");

        public static Task<string?> GetDob(string bookingId) => GetClaim(bookingId, "dob");
    }
}

