using System.Collections.Specialized;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;

namespace VAA.EAPI.Automation.Explore.Requests.Endpoints
{
    public static class GraphQL
    {
        private readonly static string _env = Config.Environment;

        #region AppointmentBooker
        public static HttpResponseMessage RetrieveAllNearByStores(string searchParameter)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQL")}"), Factory.GraphQL.RetrieveAllNearByStores(searchParameter));
        }

        public static HttpResponseMessage RetrieveStoreDetailsByID(string storeId, string origin)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQL")}"), Factory.GraphQL.RetrieveStoreDetailsByID(storeId, origin));
        }

        public static HttpResponseMessage RetrieveStoreSlotsByDate(string storeId, string date)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQL")}"), Factory.GraphQL.RetrieveStoreSlotsByDate(storeId, date));
        }

        public static HttpResponseMessage RetrieveStoreCalendarAvailability(string storeId, int year, int month)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQL")}"), Factory.GraphQL.RetrieveStoreCalendarAvailability(storeId, year, month));
        }

        public static HttpResponseMessage SaveAppointmentBooking(Bookingdetails bookingDetails)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQL")}"), Factory.GraphQL.SaveAppointmentBooking(bookingDetails));
        }
        #endregion

        #region StoreManager
        public static HttpResponseMessage StoreManagerAdminStoreById(string id, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerAdminStoreById(id), headers);
        }

        public static HttpResponseMessage StoreManagerAdminStores(string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerAdminStores(), headers);
        }

        public static HttpResponseMessage StoreManagerOpeningHours(string storeId, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerOpeningHours(storeId), headers);
        }

        public static HttpResponseMessage StoreManagerStoreById(string id, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerStoreById(id), headers);
        }

        public static HttpResponseMessage StoreManagerReplaceOpeningHours(string storeId, StoreManagerOpeningHoursReplaceRequestInput request, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerReplaceOpeningHours(storeId, request), headers);
        }

        public static HttpResponseMessage StoreManagerSlotRules(string storeId, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerSlotRules(storeId), headers);
        }

        public static HttpResponseMessage StoreManagerReplaceSlotRules(string storeId, SlotRuleReplaceRequestInput request, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerReplaceSlotRules(storeId, request), headers);
        }

        public static HttpResponseMessage StoreManagerClosures(string storeId, string from, string to, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerClosures(storeId, from, to), headers);
        }

        public static HttpResponseMessage StoreManagerCreateClosure(string storeId, StoreClosureRequestInput request, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerCreateClosure(storeId, request), headers);
        }

        public static HttpResponseMessage StoreManagerDeleteClosure(string storeId, string closureId, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerDeleteClosure(storeId,closureId), headers);
        }

        public static HttpResponseMessage StoreManagerCancelBooking(string storeId, string bookingId, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerCancelBooking(storeId, bookingId), headers);
        }

        public static HttpResponseMessage StoreManagerSlots(string storeId, string from, string to, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerSlots(storeId, from, to), headers);
        }

        public static HttpResponseMessage StoreManagerUpdateAdminStore(string id, StoreRequestInput request, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerUpdateAdminStore(id, request), headers);
        }

        public static HttpResponseMessage StoreManagerSlotAllocations(string storeId, string slotId, string startAt, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerSlotAllocations(storeId, slotId, startAt), headers);
        }

        public static HttpResponseMessage StoreManagerAvailabilityRange(string storeId, string from, string to, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerAvailabilityRange(storeId, from, to), headers);
        }

        public static HttpResponseMessage StoreManagerUpdateSlotAllocationStatus(string storeId, string slotId, string allocationId, StoreManagerSlotAllocationStatusUpdateRequestInput request, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.StoreManagerUpdateSlotAllocationStatus(storeId, slotId, allocationId, request), headers);
        }

        public static HttpResponseMessage CancelledBookings(string storeId, string date, string status, string bearer)
        {
            NameValueCollection headers = new NameValueCollection
            {
                { "Authorization", bearer }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}{ConfigReader.Get("GraphQLSecure")}"), Factory.GraphQL.CancelledBookings(storeId, date, status), headers);
        }
        #endregion
    }
}
