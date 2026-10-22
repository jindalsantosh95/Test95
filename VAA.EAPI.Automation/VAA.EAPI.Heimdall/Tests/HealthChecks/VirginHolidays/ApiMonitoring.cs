using VAA.EAPI.Automation.Core.Constants;
using Newtonsoft.Json;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using VAA.EAPI.Automation.Heimdall.Requests.Endpoints;
using VAA.EAPI.Automation.Core.Extensions;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Search.Helpers;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.VirginHolidays
{
    public class ApiMonitoring
    {
        [Fact]
        [Trait("Category", GlobalTraits.HolidaysHealthCheck)]
        [Trait("Tag", GlobalTraits.HolidaysTag)]
        public void ApiMonitoring_HolidaysFullJourney_Success()
        {
            string departureDateRaw = OfferSearchHelper.DepartureDate();
            DateTime departureDateTime = DateTime.Parse(departureDateRaw);
            string departureDate = departureDateTime.ToString("dd-MM-yyyy");
            string gateway = "LHR";
            string duration = "7";
            string bookingType = "holiday";
            string location = "orlando";

            var partyData = new List<(int Adults, List<int> ChildAges, int Infants)>
            {
                //2 Adults one child age 10
                (2, new List<int> { 10 }, 0),
            };

            //post search request to Holidays Search endpoint.
            var searchResponse = HolidaysEndpoints.HolidaysSearch(departureDate, gateway, duration, bookingType, location, partyData).EnsureIsSuccessStatusCode();

            JObject jsonObjectSearch = JsonConvert.DeserializeObject<JObject>(searchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var hotels = jsonObjectSearch["holidays"]
                .Select(result => (
                    Name: result["hotel"]?["content"]?["urlName"]?.ToString(),
                    Location: result["hotel"]?["content"]?["parentUrlName"]?.ToString()
                ))
                .Where(h => !string.IsNullOrEmpty(h.Name))
                .ToList();

            Assert.True(hotels.Count > 0, "Hotels count returned was 0");

            //Method to find a hotel to select for booking
            var selected = hotels.Find(h => h.Name.Has("Hilton"));

            string hotelName = selected.Name;

            location = selected.Location;

            //post booking request to Holidays Booking endpoint
            var bookingResponse = HolidaysEndpoints.HolidaysBooking(bookingType, location, hotelName, departureDate, duration, gateway, partyData).EnsureIsSuccessStatusCode();

            JObject jsonObjectBooking = JsonConvert.DeserializeObject<JObject>(bookingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var totalPriceBeforeDiscount = jsonObjectBooking["totalPriceBeforeDiscount"].Value<int>();

            Assert.True(totalPriceBeforeDiscount > 0, "Value returned was not > 0 as expected");

            var flightsLink = jsonObjectBooking["components"]?["flights"]?[0]?["link"]?.ToString();

            var flightsResponse = HolidaysEndpoints.HolidaysGet(flightsLink).EnsureIsSuccessStatusCode();

            JObject jsonObjectFlights = JsonConvert.DeserializeObject<JObject>(flightsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            List<string> transportUrls = jsonObjectBooking["components"]?["transport"]?["options"]
                .Select(opt => opt["link"]?.ToString())
                .Where(link => !string.IsNullOrEmpty(link))
                .ToList();

            string transferType = "Car";

            string urlToExecute = null;

            //select the URL you want for transport
            switch (transferType)
            {
                case "GenericOption":
                    urlToExecute = transportUrls.FirstOrDefault(u => u.Contains("GenericOption"));
                    break;
                case "Transfers":
                    urlToExecute = transportUrls.FirstOrDefault(u => u.Contains("Transfers"));
                    break;
                case "Car":
                    urlToExecute = transportUrls.FirstOrDefault(u => u.Contains("Car"));
                    break;
                default:
                    urlToExecute = string.Empty;
                    break;
            }

            if (urlToExecute != null)
            {
                var transportResponse = HolidaysEndpoints.HolidaysTransport(urlToExecute).EnsureIsSuccessStatusCode();
                
                JObject jsonObjectTransport = JsonConvert.DeserializeObject<JObject>(transportResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                
                //Assert
                var totalPriceBeforeDiscountFollowingRedirect = jsonObjectTransport["totalPriceBeforeDiscount"].Value<int>();

                Assert.True(totalPriceBeforeDiscountFollowingRedirect > 0, "Value returned was not > 0 as expected");

                //Get link for car Get call
                var carLink = jsonObjectTransport["components"]?["transport"]?["selectedOption"]?["link"]?.ToString();

                var getCarSelectionResponse = HolidaysEndpoints.HolidaysGet(carLink).EnsureIsSuccessStatusCode();

                JObject jsonObjectSelectedCar = JsonConvert.DeserializeObject<JObject>(getCarSelectionResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                Assert.True(totalPriceBeforeDiscount <= totalPriceBeforeDiscountFollowingRedirect, $"Values did not match when they should. Original = {totalPriceBeforeDiscount} and Post redirect = {totalPriceBeforeDiscountFollowingRedirect}");
            }

            var addonsLink = jsonObjectBooking["addonsLink"]?.ToString();

            var addonsResponse = HolidaysEndpoints.HolidaysGet(addonsLink).EnsureIsSuccessStatusCode();

            //Assert
            JObject jsonObjectAddons = JsonConvert.DeserializeObject<JObject>(addonsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(jsonObjectAddons["suppliers"].Count() > 0, "Suppliers count returned was not > 0 as expected");
        }
    }
}