using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Heimdall.Requests.Helpers.Holidays;

namespace VAA.EAPI.Automation.Heimdall.Requests.Endpoints
{
    public static class HolidaysEndpoints
    {
        private static readonly string searchUrl = "https://www.virginatlantic.com/cjs-search-api/search";
        private static readonly string bookingUrl = "https://www.virginatlantic.com/cjs-book-api/v2/booking";
        private static readonly string checkoutUrl = "https://www.virginatlantic.com/cjs-checkout-api/v2/";
        private static readonly string sessionCookie = Guid.NewGuid().ToString("N");

        public static HttpResponseMessage HolidaysSearch(string departureDate, string gateway, string duration, string bookingType, string location, List<(int Adults, List<int> ChildAges, int Infants)> partyData, bool direct = true)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Cookie", $"com.virginholidays.session={sessionCookie}" }
            };

            var searchRequest = HolidaysSearchDynamicBuilder.CreateBooking(
                departureDate: departureDate,
                gateway: gateway,
                duration: duration,
                bookingType: bookingType,
                location: location,
                direct: direct,
                partyData: partyData
            );

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            string request = JsonConvert.SerializeObject(searchRequest, settings);

            return WebRequest.Json.Post(searchUrl, request, headers);
        }

        public static HttpResponseMessage HolidaysBooking(string bookingType, string location, string hotelName, string departureDate, string duration, string gateway, List<(int Adults, List<int> ChildAges, int Infants)> partyData)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Cookie", $"com.virginholidays.session={sessionCookie}" }
            };

            var bookingRequest = HolidaysSearchDynamicBuilder.BuildBookingRequest(
                bookingType: bookingType,
                location: location,
                hotelName: hotelName,
                departureDate: departureDate,
                duration: duration,
                gateway: gateway,
                partyData: partyData
            );

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };

            string request = JsonConvert.SerializeObject(bookingRequest, settings);

            return WebRequest.Json.Post(bookingUrl, request, headers);
        }

        public static HttpResponseMessage HolidaysGet(string url)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Cookie", $"com.virginholidays.session={sessionCookie}" }
            };

            return WebRequest.Get(url, headers);
        }

        public static HttpResponseMessage HolidaysTransport(string url)
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Cookie", $"com.virginholidays.session={sessionCookie}" }
            };

            return WebRequest.Json.Put(url, "", headers);
        }

        public static HttpResponseMessage HolidaysCheckout()
        {
            var headers = new System.Collections.Specialized.NameValueCollection
            {
                { "Cookie", $"com.virginholidays.session={sessionCookie}" }
            };

            return WebRequest.Get(checkoutUrl, headers);
        }
    }
}
