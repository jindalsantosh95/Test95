using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;
using GraphQL = VAA.EAPI.Automation.Explore.Requests.Endpoints.GraphQL;

namespace VAA.EAPI.Automation.Explore.Tests.AppointmentBooker
{
    public class RetrieveStoreCalendarAvailabilityTests
    {
        [Theory]
        [Trait("Category", Traits.ExploreGraphqlRegressionAppointmentBooker)]
        [Trait("Suite", Traits.ExploreReadOnly)]
        [InlineData("1018")]
        [InlineData("1001")]
        [InlineData("1008")]
        [InlineData("1007")]
        [InlineData("1023")]
        [InlineData("1027")]
        public async Task TC01_RetrieveStoreCalendarAvailability(string storeId)
        {
            DateTime bookingDateTime = DataGenerator.RandomFutureDate(1096); // booking date from tomorrow up to 3 years ahead for real-time booking scenarios

            DateOnly randomDate = DateOnly.FromDateTime(bookingDateTime);

            int year = randomDate.Year;

            int month = randomDate.Month;

            // API CALL
            HttpResponseMessage response = GraphQL.RetrieveStoreCalendarAvailability(storeId, year, month).EnsureGQLResponseSuccess();

            RetrieveStoreCalendarAvailabilityResponse? apiResult = JsonConvert.DeserializeObject<RetrieveStoreCalendarAvailabilityResponse>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(apiResult);

            Assert.NotNull(apiResult.data);

            RetrieveStoreCalendarAvailability calendar = apiResult.data.RetrieveStoreCalendarAvailability;

            Assert.NotNull(calendar);

            // NULL VALIDATION           
            Assert.NotNull(calendar.storeId);          

            Assert.NotNull(calendar.days);

            // VALUE VALIDATION
            Assert.True(calendar.storeId == storeId, "storeId mismatch");

            Assert.True(calendar.year == year, "year mismatch");

            Assert.True(calendar.month == month, "month mismatch");

            // DAYS VALIDATION
            Assert.True(calendar.days.Length > 0, "Days list should not be empty");

            foreach (Day day in calendar.days)
            {
                Assert.True(day.date != default, "Day date is invalid");
            }

            // AVAILABILITY EXTRACTION
            bool[] availability = calendar.days.Select(d => d.hasAvailableSlot).ToArray();

            Assert.Equal(calendar.days.Length, availability.Length);
        }
    }
}
