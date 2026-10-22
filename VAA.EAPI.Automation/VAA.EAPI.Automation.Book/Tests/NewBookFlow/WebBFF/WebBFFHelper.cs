using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.PassengerBasket;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Search.Helpers;
using static VAA.EAPI.Automation.Core.Config;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF
{
    public class WebBFFHelper
    {
        private static Random rnd = new Random();

        public static void UpdateAllPassengerDetails(dynamic basket, string basketId)
        {
            HttpResponseMessage updatePassengerResponse_BFF = null;

            foreach (var item in (JArray)basket["people"])
            {
                string peopleLink = item["link"].ToString();
                string type = item["type"].ToString();

                //update basket contact details BFF request
                string firstName = $"First{DataGenerator.RandomString(5)}";
                string lastName = $"Last{DataGenerator.RandomString(5)}";
                int genderType = rnd.Next(0, 1);
                string dob = string.Empty;
                string gender = genderType.Equals(1) ? "MALE" : "FEMALE";

                switch (type)
                {
                    case "ADULT":
                        dob = DateTime.Now.AddYears(-rnd.Next(16, 100)).ToString("yyyy-MM-dd");

                        break;

                    case "TEEN":
                        dob = DateTime.Now.AddYears(-rnd.Next(12, 15)).ToString("yyyy-MM-dd");

                        break;

                    case "CHILD":
                        dob = DateTime.Now.AddYears(-rnd.Next(2, 11)).ToString("yyyy-MM-dd");

                        break;

                    case "INFANT":
                        dob = DateTime.Now.AddYears(-rnd.Next(0, 1)).ToString("yyyy-MM-dd");

                        break;

                    default: throw new ArgumentException(nameof(type));
                }

                var updatePassengerRequest = UpdatePassengerRequestBff(firstName, lastName, dob, gender, type, null, null);

                updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(basketId, peopleLink, updatePassengerRequest).EnsureIsSuccessStatusCode();
            }
            dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponse_BFF.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(((JArray)passengerBasket.people).All(x => x["validation"].ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase)), "VALID was not returned as expected for People");
        }

        public static void UpdateAllPassengerDetails(dynamic basket, TestConfig testConfig)
        {
            HttpResponseMessage updatePassengerResponse_BFF = null;

            foreach (var item in (JArray)basket["people"])
            {
                testConfig.Book.Passenger.PeopleLink = item["link"].ToString();
                testConfig.Book.Passenger.Type = item["type"].ToString();

                //update basket contact details BFF request
                testConfig.Book.Passenger.DateOfBirth = testConfig.Book.Passenger.GenerateDateOfBirth(testConfig.Book.Passenger.Type);
               
                var updatePassengerRequest = UpdatePassengerRequestBff(testConfig);

                updatePassengerResponse_BFF = WebBFFEndpoint.UpdatePassengerBFF(testConfig.Internal.BasketId, testConfig.Book.Passenger.PeopleLink, updatePassengerRequest).EnsureIsSuccessStatusCode();
            }
            dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponse_BFF.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(((JArray)passengerBasket.people).All(x => x["validation"].ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase)), $"VALID was not returned as expected for Passenger \n Basket Response : \n {passengerBasket}");
        }

        public static void UpdateValidContactDetails(string basketId, string contactDetailsLink)
        {
            //Update contact details BFF request
            string phoneNumber = "1252555111";
            string altPhoneNumber = "1252555111";
            string emailId = $"{DataGenerator.RandomString(5)}@test.com";
            bool acceptPost = false;
            bool acceptTelephone = false;
            bool acceptEmail = true;
            bool acceptTextMsg = false;
            string countryCode = "44";

            var updateContactRequest = UpdateContactRequestBff(emailId, phoneNumber, altPhoneNumber, acceptPost, acceptTelephone, acceptEmail, acceptTextMsg, countryCode);

            HttpResponseMessage updateContactResponse_BFF = WebBFFEndpoint.UpdateContactBFF(basketId, contactDetailsLink, updateContactRequest).EnsureIsSuccessStatusCode();

            dynamic contactBasket = JsonConvert.DeserializeObject<dynamic>(updateContactResponse_BFF.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.True(contactBasket.contactDetails.validation.ToString().Equals("VALID", StringComparison.OrdinalIgnoreCase), "VALID was not returned as expected for Contact Details");
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'UpdatePassengerRequestBff(TestConfig config)'.")]
        public static string UpdatePassengerRequestBff(string firstName, string lastName, string dob, string gender, string type, string flyProgram, string flyNumber)
        {
            UpdatePassenger updatePassengerReq = new UpdatePassenger();
            {
                updatePassengerReq.FirstName = firstName;
                updatePassengerReq.LastName = lastName;
                updatePassengerReq.DateOfBirth = dob;
                updatePassengerReq.Gender = gender;
                updatePassengerReq.Type = type;
                updatePassengerReq.membershipDetails = new Models.OrderCreateBookflowBFF.MembershipDetails()
                {
                    fqtProgramme = flyProgram,
                    fqtNumber = flyNumber,
                };
            }

            return JsonConvert.SerializeObject(updatePassengerReq);
        }

        public static string UpdatePassengerRequestBff(TestConfig config)
        {
            UpdatePassenger updatePassengerReq = new UpdatePassenger();
            {
                updatePassengerReq.FirstName = config.Book.Passenger.FirstName;
                updatePassengerReq.LastName = config.Book.Passenger.LastName;
                updatePassengerReq.DateOfBirth = config.Book.Passenger.DateOfBirth;
                updatePassengerReq.Gender = config.Book.Passenger.Gender;
                updatePassengerReq.Type = config.Book.Passenger.Type;
                updatePassengerReq.membershipDetails = new Models.OrderCreateBookflowBFF.MembershipDetails()
                {
                    fqtProgramme = config.Book.Passenger.FQTProgramme,
                    fqtNumber = config.Book.Passenger.FlyingClubNumber,
                };
            }

            return JsonConvert.SerializeObject(updatePassengerReq);
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'UpdateContactRequestBff(TestConfig config)'.")]
        public static string UpdateContactRequestBff(string email, string phoneNumber, string altPhoneNumber, bool acceptPost, bool acceptTelephone, bool acceptEmail, bool acceptText, string countryCode, string peopleId = "0")
        {
            UpdateContact updateContactReq = new UpdateContact();
            {
                updateContactReq.EmailAddress = email;
                updateContactReq.PhoneNumber = phoneNumber;
                updateContactReq.AlternativePhoneNumber = altPhoneNumber;
                updateContactReq.AcceptPost = acceptPost;
                updateContactReq.AcceptTelephone = acceptTelephone;
                updateContactReq.AcceptEmail = acceptEmail;
                updateContactReq.AcceptTextMessage = acceptText;
                updateContactReq.CountryCode = countryCode;
                updateContactReq.PeopleId = peopleId;
            }

            return JsonConvert.SerializeObject(updateContactReq);
        }

        public static string UpdateContactRequestBff(TestConfig config)
        {
            UpdateContact updateContactReq = new UpdateContact();
            {
                updateContactReq.EmailAddress = config.Book.Contact.EmailId;
                updateContactReq.PhoneNumber = config.Book.Contact.PhoneNumber;
                updateContactReq.AlternativePhoneNumber = config.Book.Contact.AltPhoneNumber;
                updateContactReq.AcceptPost = config.Book.Contact.AcceptPost;
                updateContactReq.AcceptTelephone = config.Book.Contact.AcceptTelephone;
                updateContactReq.AcceptEmail = config.Book.Contact.AcceptEmail;
                updateContactReq.AcceptTextMessage = config.Book.Contact.AcceptTextMsg;
                updateContactReq.CountryCode = config.Book.Contact.CountryCode;
                updateContactReq.PeopleId = config.Book.Contact.PeopleId;
            }

            return JsonConvert.SerializeObject(updateContactReq);
        }
        

        public static string UpdateTnCRequestBff(bool accepted)
        {
            UpdateTnC updateTnCReq = new UpdateTnC();
            {
                updateTnCReq.Accepted = accepted;
            }

            return JsonConvert.SerializeObject(updateTnCReq);
        }

        public static string Payment(string payWithPointsValue = null)
        {
            Models.OrderCreateBookflowBFF.Payment paymentReq = new Models.OrderCreateBookflowBFF.Payment();
            {
                paymentReq.ReturnUrl = "https://stg.virginatlantic.com/payment/captured";
                paymentReq.ExpiryUrl = "https://www.google.com";
                paymentReq.Link = "";
                paymentReq.PayWithPointsValue = payWithPointsValue;
            }

            return JsonConvert.SerializeObject(paymentReq);
        }

        public static void SummaryURL(string url, out string basketId, out string slice, out string offerId)
        {
            basketId = url.Split('/')[5];
            slice = url.Split('/')[7];
            offerId = url.Split('/').LastOrDefault();
        }

        public static string BuildSummaryUrl(string basketId, string slice, string offerId) => $"{TestResourcesVirginAtlantic.BaseUrl}/{Config.Environment}/search/summary/{basketId}/{slice}|{offerId}";

        public static void ProductionSummaryUrl(string url, out string basketId, out string slice, out string offerId)
        {
            basketId = url.Split('/')[4];
            slice = url.Split('/')[6];
            offerId = url.Split('/').LastOrDefault();
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'HPPUrl(TestConfig config)'.")]
        public static void HPPUrl(string hppUrl, out string pri, out string orderNo)
        {
            pri = hppUrl.Split('=')[1];
            orderNo = pri.Substring(0, 20);
        }

        public static void HPPUrl(TestConfig config)
        {
            config.Book.Order.PRI = config.Book.Order.HppUrl.Split('=')[1];
        }

        public static string GetSortLinkBySortName(string jsonResponse, string sortName)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string sortLink = jsonObject.SelectToken($"$.result.flightSortLinks.links[?(@.name == '{sortName}')].href").ToString();

            return sortLink;
        }

        public static string GetFilterLinkByFilterName(string jsonResponse, string filterName)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string filterLink = jsonObject.SelectToken($"$.result.flightSearchFilters.filters[*].values[?(@.displayName == '{filterName}')].links[*].href").ToString();

            return filterLink;
        }

        public static void ConfirmationResponse(dynamic confirmationResult, out string pnr, out double totalPrice, out string email, out string currency)
        {
            pnr = confirmationResult.summary.outboundFlights[0].sectors[0].pnr.Value;
            totalPrice = confirmationResult.summary.totalPrice.Value;
            email = confirmationResult.leadPassenger.contact.emailAddress.Value;
            currency = confirmationResult.summary.priceCurrency.Value;
        }

        public static void ConfirmationInfo(HttpResponseMessage orderCreateGQLResponse, TestConfig config)
        {
            dynamic confirmationResponse = JsonConvert.DeserializeObject<dynamic>(orderCreateGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            JObject confirmationRes = JObject.Parse(orderCreateGQLResponse.ReadResponseAsString());

            var outBoundcabinClass = string.Empty;
            var inBoundcabinClass = string.Empty;

            var outBoundcabinClassArray = confirmationRes.SelectTokens("$.summary.slices[0].cabinClass").Select(x => x.ToString()).ToArray();
            var inBoundcabinClassArray = confirmationRes.SelectTokens("$.summary.slices[1].cabinClass").Select(x => x.ToString()).ToArray();

            foreach (var item in outBoundcabinClassArray)
                outBoundcabinClass = item + ",";
            foreach (var inbound in inBoundcabinClassArray)
                inBoundcabinClass = inbound + ",";

            //Price Details 
            config.Book.Cost.TotalPrice = confirmationResponse.summary.totalPrice.Value;
            config.Book.Cost.Currency = confirmationResponse.summary.priceCurrency.Value;
            
            //PNR
            config.Book.Order.PNR = confirmationResponse.summary.slices[0].sectors[0].pnr.Value;

            //CabinClass
            config.Book.Order.OutBoundCabinClass = outBoundcabinClass;
            config.Book.Order.InBoundCabinClass = inBoundcabinClass;

            //Passenger details 
            config.Book.Order.EmailId = confirmationResponse.leadPassenger.contact.emailAddress.Value;
            config.Book.Order.LeadPassengerFirstName = confirmationResponse.leadPassenger.firstName.Value;
            config.Book.Order.LeadPassengerLastName = confirmationResponse.leadPassenger.lastName.Value;
            config.Book.Order.PassengerParty = confirmationRes.SelectToken("$.summary.partyComposition").ToString();
            config.Book.Order.PassengersCount = confirmationRes.SelectToken("$.summary.partyComposition.total").ToString();
            config.Book.Order.FlightPassengerSeatMap = confirmationRes.SelectToken("$.summary.flightPassengerSeatMap").ToString();
            config.Book.Order.DepartureDate = confirmationRes.SelectToken("$.summary.departureDate").ToString();

            //Flying Club number details 
            config.Book.Order.FcPointsPostBooking = confirmationRes.SelectToken("$.customerProfile.membershipDetails.fqtPoints").ToString();
            config.Book.Order.FqtTierCode = confirmationRes.SelectToken("$.customerProfile.membershipDetails.fqtTierCode").ToString();
            config.Book.Order.FqtProgramme = confirmationRes.SelectToken("$.customerProfile.membershipDetails.fqtProgramme").ToString();
            config.Book.Order.FlyingClubNumber = confirmationRes.SelectToken("$.customerProfile.membershipDetails.fqtNumber").ToString();
            config.Book.Order.FlightPoints = confirmationRes.SelectToken("$.summary.membershipPoints").ToString();

        }

        public static void GetOfferIdAndSliceFromSummaryURL(string url, out string slice, out string offerId)
        {
            slice = url.Split('/')[7];
            offerId = url.Split('/').LastOrDefault();
        }

        public static void OfferSearchBFF(JourneyType journeyType, FlightScenarioType flightScenarioType, LocationType origin, LocationType destination, int adults, int teens, int child, int infants, bool rewardFlag, out string basketId, out HttpResponseMessage summaryBffResponse)
        {
            if (journeyType == JourneyType.OneWay)
            {
                //Generating offer search BFF request
                var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(journeyType, origin: origin, destination: destination, adults: adults, teens: teens, child: child, infants: infants, rewardFlag: rewardFlag);

                //Post search BFF endpoint
                HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

                var result = offerSearchBFFResponse.EnsureIsSuccessStatusCode().ReadResponseAsString();

                //Getting the link url for the SummaryBff
                var url = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForOneWay(result, flightScenarioType);

                //Getting basketId,slice and offerId from the summary url
                SummaryURL(url, out basketId, out string slice, out string offerId);

                //Need to build url for Summary link from returned url
                url = BuildSummaryUrl(basketId, slice, offerId);

                //Post to summary endpoint taken from search result
                summaryBffResponse = WebBFFEndpoint.SummaryBFF(url).EnsureIsSuccessStatusCode();
            }
            else
            {
                //Generating offer search BFF request
                var offerSearchBFFRequest = OfferSearchHelper.OfferSearchBFFRequest(journeyType, origin: origin, destination: destination, returnOrigin: destination, returnDestination: origin, adults: adults, teens: teens, child: child, infants: infants, rewardFlag: rewardFlag);

                //Post search BFF endpoint
                HttpResponseMessage offerSearchBFFResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFRequest).EnsureIsSuccessStatusCode();

                var result = offerSearchBFFResponse.ReadResponseAsString();

                var url = OfferSearchHelper.FindOfferForReturn(result, flightScenarioType);

                //Post Offer URL
                HttpResponseMessage postUrlResponse = WebBFFEndpoint.PostOffer(url, "");

                dynamic postOfferResponse = JsonConvert.DeserializeObject<dynamic>(postUrlResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                //Getting the Id and FareId from the response of Select and Next to build up URL for Summary
                string id = postOfferResponse.slice.id.Value.ToString();
                string fareId = postOfferResponse.slice.fareId.Value.ToString();

                //Need to call search and this time pass in the id and fareId details into the body
                var offerSearchBFFReturnRequest = OfferSearchHelper.OfferSearchBFFRequestReturn(JourneyType.Return, origin: origin, destination: destination, returnOrigin: destination, returnDestination: origin, adults: adults, teens: teens, child: child, infants: infants, id: id, fareId: fareId, rewardFlag: rewardFlag);

                //Post search BFF endpoint
                HttpResponseMessage offerSearchBFFReturnResponse = WebBFFEndpoint.OfferSearchBFF(offerSearchBFFReturnRequest).EnsureIsSuccessStatusCode();

                var inboundResult = offerSearchBFFReturnResponse.ReadResponseAsString();

                //Getting the link url for the SummaryBff
                var summaryUrl = OfferSearchHelper.FindSummaryLinkForSpecifiedAirlineCodeAndCabinNameForReturn(inboundResult, flightScenarioType);

                //Getting basketId,slice and offerId from the summary url
                SummaryURL(summaryUrl, out basketId, out string slice, out string offerId);

                //Need to build url for Summary link from returned url
                summaryUrl = BuildSummaryUrl(basketId, slice, offerId);

                //Post to summary endpoint taken from search result
                summaryBffResponse = WebBFFEndpoint.SummaryBFF(summaryUrl).EnsureIsSuccessStatusCode();
            }
        }
    }
}