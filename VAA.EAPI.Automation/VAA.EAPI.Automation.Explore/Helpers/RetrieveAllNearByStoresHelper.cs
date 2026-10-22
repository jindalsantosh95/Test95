using VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker;

namespace VAA.EAPI.Automation.Explore.Helpers
{
    public static class RetrieveAllNearByStoresHelper
    {
        // 1. Validate list exists
        public static void ValidateStoreList(Store[] apiStores)
        {
            Assert.NotNull(apiStores);

            Assert.True(apiStores.Length > 0);
        }

        // 2. Validate isClosest rule
        public static void ValidateIsClosestRule(Store[] apiStores)
        {
            Assert.NotNull(apiStores);

            Assert.True(apiStores.Length > 0);

            Assert.True(apiStores[0].isClosest);
        }

        // 3. Validate basic store fields
        public static void ValidateStoreBasicFields(Store apiStore, StoreMasterRecord dbStore)
        {
            Assert.NotNull(apiStore);

            Assert.NotNull(dbStore);

            Assert.Equal(dbStore.StoreId, apiStore.storeId);

            Assert.Equal(dbStore.Name, apiStore.name);

            Assert.Equal(dbStore.Label, apiStore.locationLabel);
        }

        // 4. Validate fullDetails only if present
        public static void ValidateFullDetails(Store apiStore, StoreMasterRecord dbStore)
        {
            if (apiStore.fullDetails == null)
                return;

            Details details = apiStore.fullDetails.details;

            if (details != null)
            {
                Assert.Equal(dbStore.Phone, details.phone);

                Assert.Equal(dbStore.EmailAddress, details.emailAddress);

                Address addr = details.address;

                if (addr != null)
                {
                    Assert.Equal(dbStore.AddressLine1, addr.addressLine1);

                    Assert.Equal(dbStore.AddressLine2, addr.addressLine2);

                    Assert.Equal(dbStore.City, addr.city);

                    Assert.Equal(dbStore.County, addr.county);

                    Assert.Equal(dbStore.PostalCode, addr.postalCode);
                }
            }

            Openinghours hours = apiStore.fullDetails.openingHours;

            if (hours != null)
            {
                RetrieveStoreDetailsByIDHelper.ValidateWeeklyHours(hours.weeklyHours,dbStore.WeeklyHours);
            }

            Services services = apiStore.fullDetails.services;

            if (services != null)
            {
                RetrieveStoreDetailsByIDHelper.ValidateServices(services.inStoreFacilities,dbStore);
            }
        }
    }
}
