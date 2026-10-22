using NodaTime;
using System.Security.Claims;
using VAA.EAPI.GraphQL.Common.Helper;
using CommonGraphql = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    public class BookingIdHelper : BaseIdHelper
    {
        public static async Task<string> CreateBookingId(string? existingBookingId, string? msBookingId, string? givenName, string? surName, string? pnrLocator = null, string? ticketNumber = null, string? basketId = null, string? dob = null)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            var claims = new List<Claim> { };

            claims = await MergeClaim(claims, existingBookingId, jwtKey, "msBookingId", msBookingId);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "givenName", givenName);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "surName", surName);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "pnrLocator", pnrLocator);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "ticketNumber", ticketNumber);
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "basketId", basketId ?? Guid.NewGuid().ToString());
            claims = await MergeClaim(claims, existingBookingId, jwtKey, "dob", dob);

            return await GetTokenString(claims, DateTime.UtcNow.AddDays(1), jwtKey);
        }

        public static async Task<string?> GetMSBookingId(string bookingId)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            return await GetClaim(bookingId, jwtKey, "msBookingId");
        }

        public static async Task<string?> GetGivenName(string bookingId)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            return await GetClaim(bookingId, jwtKey, "givenName");
        }

        public static async Task<string?> GetPnrLocator(string bookingId)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            return await GetClaim(bookingId, jwtKey, "pnrLocator");
        }

        public static async Task<string?> GetTicketNumber(string bookingId)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            return await GetClaim(bookingId, jwtKey, "ticketNumber");
        }

        public static async Task<string?> GetSurName(string bookingId)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            return await GetClaim(bookingId, jwtKey, "surName");
        }
        public static async Task<string?> GetBasketId(string bookingId)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            return await GetClaim(bookingId, jwtKey, "basketId");
        }

        public static async Task<string?> GetDOB(string bookingId)
        {
            var jwtKey = ConfigReader.Get(CommonGraphql.BookingIdJwtKey);
            return await GetClaim(bookingId, jwtKey, "dob");
        }
        
        public static bool IsHolidayBookingExpired(VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.Booking? booking)
        {
            if (booking?.TravelDates?.ReturnDate is not { } returnDate)
                return false;

            var daysSinceReturn = Period
                .Between(returnDate.Date, LocalDate.FromDateTime(DateTime.UtcNow), PeriodUnits.Days)
                .Days;

            return daysSinceReturn >= 7;
        }
    }
}
