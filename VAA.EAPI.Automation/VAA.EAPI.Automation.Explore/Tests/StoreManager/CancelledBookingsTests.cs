using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Helpers;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.StoreManager
{
    public class CancelledBookingsTests : IClassFixture<TokenFixture>
    {
        private readonly TokenFixture _tokenFixture;

        public CancelledBookingsTests(TokenFixture tokenFixture)
        {
            _tokenFixture = tokenFixture;

            _tokenFixture.AddTokenForExploreHolidaysApplicationFromKeyVault("GraphToken");
        }

        [Theory]
        [MemberData(nameof(GetActiveStoreIds))]
        [Trait("Category", Traits.ExploreGraphqlRegressionStoreManager)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        public async Task TC01_CancelledBookings(string storeId)
        {
            DateTime currentDate = DateTime.UtcNow.Date;

            DateTime nextDate = currentDate.AddDays(1);

            string bearer = _tokenFixture.GetToken("GraphToken");

            await ValidateBookings(storeId, currentDate, "BOOKED", bearer);

            await ValidateBookings(storeId, currentDate, "CANCELLED", bearer);

            await ValidateBookings(storeId, nextDate, "BOOKED", bearer);

            await ValidateBookings(storeId, nextDate, "CANCELLED", bearer);
        }

        private static async Task ValidateBookings(string storeId, DateTime date, string status, string bearer)
        {
            SoftAssertHelper softAssert = new();

            string dateString = date.ToString("yyyy-MM-dd");

            // Step 1: Call the CancelledBookings query for the store, date, and status.
            HttpResponseMessage response = GraphQL.CancelledBookings(storeId, dateString, status, bearer).EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();

            CancelledBookingsResponse? apiResult = JsonConvert.DeserializeObject<CancelledBookingsResponse>(responseBody);

            Assert.NotNull(apiResult);

            CancelledBooking[] apiBookings = apiResult.data?.storeManagerBookings ?? Array.Empty<CancelledBooking>();

            // Step 2: Retrieve the expected bookings from the database.
            List<CancelledBookingDbRecord> dbBookings = CancelledBookingsDbHelper.GetBookings(storeId, date, status);

            // Step 3: Compare the API and database booking counts.
            softAssert.AreEqual(dbBookings.Count, apiBookings.Length, $"API and database booking count mismatch for store {storeId}, date {dateString}, status {status}");

            TimeZoneInfo ukTimeZone = GetUkTimeZone();

            foreach (CancelledBooking apiBooking in apiBookings)
            {
                // Step 4: Validate the API booking structure and business values.
                softAssert.True(!apiBooking.id.IsNullOrWhiteSpace(), $"Booking ID is empty for store {storeId}");

                softAssert.AreEqual(storeId, apiBooking.storeId, $"Store ID mismatch for booking {apiBooking.id}");

                softAssert.True(!apiBooking.storeSlotId.IsNullOrWhiteSpace(), $"StoreSlotId is empty for booking {apiBooking.id}");

                softAssert.AreEqual(status, apiBooking.status, $"Status mismatch for booking {apiBooking.id}");

                softAssert.True(apiBooking.windowStartAt < apiBooking.windowEndAt, $"Invalid booking window for booking {apiBooking.id}");

                softAssert.True(apiBooking.windowCapacity > 0, $"Invalid window capacity for booking {apiBooking.id}");

                softAssert.True(apiBooking.createdAt <= apiBooking.updatedAt, $"UpdatedAt is earlier than CreatedAt for booking {apiBooking.id}");

                DateTime apiBookingLocalDate = TimeZoneInfo.ConvertTime(apiBooking.windowStartAt, ukTimeZone).Date;

                softAssert.AreEqual(date.Date, apiBookingLocalDate, $"Booking date mismatch for booking {apiBooking.id}");

                // Step 5: Match the API booking with the expected database booking.
                CancelledBookingDbRecord? matchingDbBooking = dbBookings.FirstOrDefault(databaseBooking =>
                    databaseBooking.BookingId == apiBooking.id);

                softAssert.True(matchingDbBooking != null, $"Database booking {apiBooking.id} was not returned by the API");

                if (matchingDbBooking == null)
                {
                    continue;
                }

                // Step 6: Validate database expected values against API actual values.
                softAssert.AreEqual(matchingDbBooking.BookingId, apiBooking.id, $"Booking ID mismatch for booking {apiBooking.id}");

                softAssert.AreEqual(matchingDbBooking.StoreId, apiBooking.storeId, $"Store ID mismatch for booking {apiBooking.id}");

                softAssert.AreEqual(matchingDbBooking.StoreSlotId?.ToString(), apiBooking.storeSlotId, $"StoreSlotId mismatch for booking {apiBooking.id}");

                string? expectedCustomerRef = matchingDbBooking.CustomerRef.IsNullOrWhiteSpace() ? null : matchingDbBooking.CustomerRef.Trim();

                string? actualCustomerRef = apiBooking.customerRef.IsNullOrWhiteSpace() ? null : apiBooking.customerRef.Trim();

                softAssert.AreEqual(expectedCustomerRef, actualCustomerRef, $"CustomerRef mismatch for booking {apiBooking.id}");

                softAssert.AreEqual(matchingDbBooking.Status, apiBooking.status, $"Database and API status mismatch for booking {apiBooking.id}");

                // Database slot values represent UK local slot time despite the Utc field name.
                DateTime expectedStartAtUtc = ConvertDatabaseSlotTimeToUtc(matchingDbBooking.StartsAtUtc);

                DateTime expectedEndAtUtc = ConvertDatabaseSlotTimeToUtc(matchingDbBooking.EndsAtUtc);

                softAssert.AreEqual(expectedStartAtUtc, apiBooking.windowStartAt.UtcDateTime, $"Window startAt mismatch for booking {apiBooking.id}");

                softAssert.AreEqual(expectedEndAtUtc, apiBooking.windowEndAt.UtcDateTime, $"Window endAt mismatch for booking {apiBooking.id}");

                softAssert.AreEqual(matchingDbBooking.Capacity, apiBooking.windowCapacity, $"Window capacity mismatch for booking {apiBooking.id}");

                softAssert.AreEqual(matchingDbBooking.CreatedUtc.UtcDateTime, apiBooking.createdAt.UtcDateTime, $"CreatedAt mismatch for booking {apiBooking.id}");

                softAssert.AreEqual(matchingDbBooking.UpdatedUtc.UtcDateTime, apiBooking.updatedAt.UtcDateTime, $"UpdatedAt mismatch for booking {apiBooking.id}");
            }

            softAssert.AssertAll();
        }

        private static DateTime ConvertDatabaseSlotTimeToUtc(DateTimeOffset databaseSlotTime)
        {
            TimeZoneInfo ukTimeZone = GetUkTimeZone();

            DateTime localSlotTime = DateTime.SpecifyKind(databaseSlotTime.DateTime, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(localSlotTime, ukTimeZone);
        }

        private static TimeZoneInfo GetUkTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            }
        }

        public static IEnumerable<object[]> GetActiveStoreIds()
        {
            List<string> storeIds = ExploreStoreMasterDBHelper.GetAllActiveStoreIds();

            Assert.NotEmpty(storeIds);

            foreach (string storeId in storeIds)
            {
                yield return new object[] { storeId };
            }
        }
    }
}
