using VAA.EAPI.Automation.Explore.Constants;
using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class RetrieveStoreDetailsByIDHelper
    {
        public static string NormalizeHours(string opening, string closing)
        {
            // DB convention: 00:00 - 00:00 means closed
            if (opening == "00:00" && closing == "00:00")
                return "Closed";

            return $"{opening} - {closing}";
        }

        // WEEKLY HOURS
        public static void ValidateWeeklyHours(Weeklyhour[] apiWeekly, List<DbWeeklyHour> dbWeekly)
        {
            Assert.NotNull(apiWeekly);

            Assert.NotNull(dbWeekly);

            Assert.Equal(dbWeekly.Count, apiWeekly.Length);

            int i = 0;

            foreach (DbWeeklyHour hour in dbWeekly)
            {
                string expectedHours = NormalizeHours(hour.openingTime, hour.closingTime);

                Assert.Equal(hour.day, apiWeekly[i].day);

                Assert.Equal(expectedHours, apiWeekly[i].hours);

                i++;
            }            
        }

        // SERVICES
        public static void ValidateServices(string[] apiFacilities, StoreMasterRecord dbStore)
        {
            Assert.NotNull(apiFacilities);
            Assert.NotNull(dbStore);

            List<string> expectedFacilities = new();

            if (dbStore.HasKidsArea)
                expectedFacilities.Add(Application.ServiceKids);

            if (dbStore.HasWelcomeDrinksAndSnacks)
                expectedFacilities.Add(Application.ServiceFood);

            if (dbStore.HasAircraftSeats)
                expectedFacilities.Add(Application.ServiceAircraft);

            if (dbStore.HasSecretCinema)
                expectedFacilities.Add(Application.ServiceCinema);

            Assert.Equal(expectedFacilities.Count, apiFacilities.Length);

            foreach (string facility in expectedFacilities)
            {
                Assert.True(Application.ContainsService(apiFacilities, facility));
            }
        }
    }
}
