using System.Collections.Specialized;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Trips.Requests.Endpoint
{
    public static class GraphQL
    {
        public static HttpResponseMessage Booking(string env, string usingString, string pnr, string givenNames, string surname)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.Booking(usingString, pnr, givenNames, surname));
        }

        public static HttpResponseMessage GetFlyReadyStatus(string env, string bookingId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.GetFlyReadyStatus(bookingId));
        }

        public static HttpResponseMessage GetSpecialAssistance(string env, string bookingId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.GetSpecialAssistance(bookingId));
        }

        public static HttpResponseMessage GetSpecialMeals(string env, string bookingId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.GetSpecialMeals(bookingId));
        }

        public static HttpResponseMessage UpdateSpecialMeals(string env, string bookingId, string passengerId, string segmentId, string ssrCode, bool applyToAll)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.UpdateSpecialMeals(bookingId, passengerId, segmentId, ssrCode, applyToAll));
        }

        public static HttpResponseMessage SelectSpecialAssistance(string env, string bookingId, string passengerResourceId, List<string> segmentIds, string ssrCode, string ssrCategoryCodeString)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.SelectSpecialAssistance(bookingId, passengerResourceId, segmentIds, ssrCode, ssrCategoryCodeString));
        }

        public static HttpResponseMessage LoyaltyProgramsDetails(string env)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.LoyaltyProgramsDetails());
        }

        public static HttpResponseMessage UpdateLoyaltyMemberDetails(string env, string bookingId, string loyaltyMemberId, string loyaltyProgramCode, string leadPassengerId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.UpdateLoyaltyMemberDetails(bookingId, loyaltyMemberId, loyaltyProgramCode, leadPassengerId));
        }

        public static HttpResponseMessage GetSeatMap(string env, string bookingId, string segmentId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.GetSeatMap(bookingId, segmentId));
        }

        public static HttpResponseMessage SelectSeat(string env, string bookingId, string segmentId, string passengerId, string seatNumber, string seatOfferId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.SelectSeatMutation(bookingId, segmentId, passengerId, seatNumber, seatOfferId));
        }

        public static HttpResponseMessage SeatPayment(string env, string bookingId)
        {
            NameValueCollection headers = new()
            {
                { "X-Forwarded-For", "0.0.0.0" }
            };

            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.SeatPaymentMutation(bookingId), headers);
        }

        public static HttpResponseMessage SeatFulfilmentMutation(string env, string bookingId, string paymentStatus)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.SeatFulfilmentMutation(bookingId, paymentStatus));
        }

        public static HttpResponseMessage Content(string env, string contentFragmentId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{(env.Is("Prod") ? string.Empty : $"{env}")}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.Content(contentFragmentId));
        }

        public static HttpResponseMessage GetContentFragment(string env, string hotelId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{(env.Is("Prod") ? string.Empty : $"{env}")}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.GetContentFragment(hotelId));
        }
        public static HttpResponseMessage GetUpsellOffers(string env, string bookingId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.GetUpsellOffers(bookingId));
        }

        public static HttpResponseMessage TripDetails(string env, List<string> pnrs, List<string> givenNames, List<string> surnames, List<string>? createdDates)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.TripDetails(pnrs, givenNames, surnames, createdDates));
        }

        /// <summary>
        /// Mixed-shape <c>TripDetails</c> lookup — each entry can carry either a
        /// PNR-style or a Holidays-booking-style identification. See
        /// <see cref="Factory.TripDetailEntry"/>.
        /// </summary>
        public static HttpResponseMessage TripDetails(string env, List<Factory.TripDetailEntry> entries)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.TripDetails(entries));
        }

        public static HttpResponseMessage FindTripByFlightDate(string env, string pnr, string flightDate, string givenName, string surname)
        {
            return WebRequest.Json.Post(
                Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"),
                Factory.GraphQL.FindTripByFlightDate(pnr, flightDate, givenName, surname));
        }

        public static HttpResponseMessage GetDeepLinksResponse(string env, string bookingId, string deeplinkOption, string segmentId = null)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.GetDeepLinks(bookingId, deeplinkOption, segmentId));
        }

        public static HttpResponseMessage GetDeepLinksSecureResponse(string env, string cacheId, string ocpSubKey)
        {
            NameValueCollection headers = new()
            {
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("tripsSecureEndpoint")}"), Factory.GraphQL.GetDeepLinksSecure(cacheId), headers);
        }

        public static HttpResponseMessage AccountMemberSignIn(string env, string loyaltyNum, string password)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.AccountMemberSignIn(loyaltyNum, password));
        }

        public static HttpResponseMessage AccountMemberSignInVAC(string env, string emailVAC, string password)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.AccountMemberSignInVAC(emailVAC, password));
        }

        public static HttpResponseMessage TripDetailsLoggedInUserToken(string env, string idToken)
        {
            NameValueCollection headers = new()
            {
                { "Authorization", idToken }
            };
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.TripDetailsLoggedInUserToken(), headers);
        }

        public static HttpResponseMessage RetrieveHolidayBooking(string env, string usingString, string bookingReference, string dateOfBirth, string surname)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.RetrieveHolidayBooking(usingString, bookingReference, dateOfBirth, surname));
        }

        public static HttpResponseMessage HolidayPaymentMutation(string env, string tripResourceId, decimal amountToPay)
        {

            return WebRequest.Json.Post(
                Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"),
                Factory.GraphQL.HolidayPaymentMutation(tripResourceId, amountToPay));
        }

        public static HttpResponseMessage RetrieveHolidayBookingDocumentDetails(string env, string tripResourceId)
        {
            return WebRequest.Json.Post(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"), Factory.GraphQL.RetrieveHolidayBookingDocumentDetails(tripResourceId));
        }

        public static HttpResponseMessage RetrieveHolidayBookingDocument(string env, string documentId, string documentToken)
        {
            return WebRequest.Json.Post(
                Config.DigitalTestVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQL")}"),
                Factory.GraphQL.RetrieveHolidayBookingDocument(documentId, documentToken));
        }
    }
}
