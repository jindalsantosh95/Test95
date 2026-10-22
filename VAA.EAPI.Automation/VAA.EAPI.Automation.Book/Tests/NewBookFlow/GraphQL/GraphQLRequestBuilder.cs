using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.ContactDetailsBasket;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.OrderConfirmation;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.PassengerBasket;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.PaymentRetrieve;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.PaymentStatus;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.RetrieveBasket;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.RetrieveFQTProgramme;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.RetrieveSummary;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.SeatGQL.RetrieveSeatMap;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.SeatGQL.SelectionSeat;
using VAA.EAPI.Automation.Book.Models.OrderCreateBookflowBFF.GraphQL.TnCBasket;
using VAA.EAPI.Automation.Book.Requests.Endpoints;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.Xunit;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL
{
    public class GraphQLRequestBuilder
    {

        public static string FQTProgrammeGQLRequest()
        {
            RetrieveFQTProgramme retrieveFQTRequest = new()
            {
                pqId = "retrieveFQTProgrammes"
            };

            return JsonConvert.SerializeObject(retrieveFQTRequest);
        }

        public static string RetrieveBasketGQLRequest(string basketId)
        {
            BasketGQLRequest retrieveBasketGQLRequest = new()
            {
                pqId = "retrieveBasketDetailsPqId",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.RetrieveBasket.Variables()
                {
                    basketId = basketId
                }
            };

            return JsonConvert.SerializeObject(retrieveBasketGQLRequest);
        }

        public static string RetrieveSummaryGQLRequest(string basketId)
        {
            RetrieveSummaryGQLRequest RetrieveSummaryGQLRequest = new()
            {
                pqId = "retrieveSummaryResponse",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.RetrieveSummary.Variables()
                {
                    basketId = basketId
                }
            };

            return JsonConvert.SerializeObject(RetrieveSummaryGQLRequest);
        }

        public static string TnCBasketGQLRequest(string basketId)
        {
            TnCBasketGQLRequest TnCBasketRequest = new()
            {
                pqId = "updateTnCMutationPqId",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.TnCBasket.Variables()
                {
                    updateTnCRequest = new UpdateTnCRequest()
                    {
                        termsAndConditions = new TermsAndConditions()
                        {
                            accepted = true
                        },
                        basketId = basketId
                    }
                }
            };

            return JsonConvert.SerializeObject(TnCBasketRequest);
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'ContactBasketGraphQLRequest(TestConfig config)'.")]
        public static string ContactBasketGraphQLRequest(string basketId, string email, string phoneNumber, string altPhoneNumber, bool acceptPost, bool acceptTelephone, bool acceptEmail, bool acceptText, string countryCode)
        {
            ContactBasketGQLRequest contactBasketGQLRequest = new()
            {
                pqId = "updateContactDetailsPqId",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.ContactDetailsBasket.Variables()
                {
                    updateContactDetailsRequest = new UpdateContactDetailsRequest()
                    {
                        basketId = basketId,
                        contactDetails = new ContactDetails()
                        {
                            emailAddress = email,
                            phoneNumber = phoneNumber,
                            alternativePhoneNumber = altPhoneNumber,
                            acceptPost = acceptPost,
                            acceptTelephone = acceptTelephone,
                            acceptEmail = acceptEmail,
                            acceptTextMessage = acceptText,
                            validation = "VALID",
                            countryCode = countryCode,
                            peopleId = "0"
                        }
                    },
                }
            };

            return JsonConvert.SerializeObject(contactBasketGQLRequest);
        }

        public static string ContactBasketGraphQLRequest(TestConfig config)
        {
            ContactBasketGQLRequest contactBasketGQLRequest = new()
            {
                pqId = "updateContactDetailsPqId",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.ContactDetailsBasket.Variables()
                {
                    updateContactDetailsRequest = new UpdateContactDetailsRequest()
                    {
                        basketId = config.Internal.BasketId,
                        contactDetails = new ContactDetails()
                        {
                            emailAddress = config.Book.Contact.EmailId,
                            phoneNumber = config.Book.Contact.PhoneNumber,
                            alternativePhoneNumber = config.Book.Contact.AltPhoneNumber,
                            acceptPost = config.Book.Contact.AcceptPost,
                            acceptTelephone = config.Book.Contact.AcceptTelephone,
                            acceptEmail = config.Book.Contact.AcceptEmail,
                            acceptTextMessage = config.Book.Contact.AcceptTextMsg,
                            validation = "VALID",
                            countryCode = config.Book.Contact.CountryCode
                        }
                    },
                }
            };

            return JsonConvert.SerializeObject(contactBasketGQLRequest);
        }

        public static void PassengerBasketGraphQLRequest(dynamic basket, string basketId, bool isFrequentFlyer = false, string tier = "", string b2cToken = "", string passengerFirstName = "", string passengerLastName = "", string partnerFqtProgramme = "", string partnerFqtNumber = "")
        {
            HttpResponseMessage updatePassengerResponseGQL = null;

            var people = (JArray)basket.data.retrieveBasket.people;
            bool isAwardSearch = basket.data.retrieveBasket.summary.isAwardSearch.Value;
            DateTime latestTravelDate = ResolveLatestTravelDate(basket);

            int passengerIndex = 0;

            foreach (var item in people)
            {
                string passengerId = item["id"].ToString();
                string type = item["type"].ToString();

                bool isProfilePassenger = item["profilePassenger"] != null && item["profilePassenger"].Type != JTokenType.Null
                                ? (bool)item["profilePassenger"]
                                : false;

                if (passengerId == "0" && isProfilePassenger) continue;

                string firstName;
                string lastName;
                string dob;
                string gender = (Random.Shared.Next(0, 2) == 1) ? "MALE" : "FEMALE";
                string fqtProgramme = "";
                string fqtNumber = "";

                if (isFrequentFlyer && passengerId == "0")
                {
                    var loyaltyDetails = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints(tier);

                    firstName = loyaltyDetails["firstName"];
                    lastName = loyaltyDetails["lastName"];
                    dob = loyaltyDetails["dob"];
                    fqtNumber = loyaltyDetails["rowKey"];
                    fqtProgramme = "VS";
                }
                else
                {
                    // Use provided name for first passenger
                    if (passengerIndex == 0 && !string.IsNullOrEmpty(passengerFirstName) && !string.IsNullOrEmpty(passengerLastName))
                    {
                        firstName = passengerFirstName;
                        lastName = passengerLastName;
                    }
                    else
                    {
                        firstName = DataGenerator.RandomFirstName();
                        lastName = DataGenerator.RandomLastName();
                    }

                    dob = GraphQLHelper.GenerateDOBByPassengerType(type, latestTravelDate);

                    if (passengerId == "0" &&
                        !partnerFqtProgramme.IsNullOrWhiteSpace() &&
                        !partnerFqtNumber.IsNullOrWhiteSpace())
                    {
                        fqtProgramme = partnerFqtProgramme;
                        fqtNumber = partnerFqtNumber;
                    }
                }
                var updatePassengerRequest = UpdatePassengerRequestGQL(basketId, passengerId, firstName, lastName, dob, gender, type, fqtProgramme, fqtNumber);

                updatePassengerResponseGQL = GraphQLEndpoint.Book(updatePassengerRequest, b2cToken).EnsureGQLResponseSuccess();

                dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponseGQL.Content.ReadAsStringAsync().GetAwaiter().GetResult());

                var softAssertPassenger = new SoftAssertHelper();

                softAssertPassenger.True(GraphQLAssertion.ValidatePassengerBasketMembershipDetails(passengerBasket, basketId, passengerId, out string message), $"Basket Validation failed in passegner : \n{message}");

                if (isAwardSearch)
                {
                    //MemberShip points valdation with offer price data
                    softAssertPassenger.True(passengerBasket.data.updatePeople.summary.isAwardSearch.Value, "Passenger Validation : isAwardSearch is not True for redemption points scenario");

                    softAssertPassenger.True(passengerBasket.data.updatePeople.summary.membershipPoints.Value != null, $"Passenger Validation : membershipPoints field is null  \n Response : {passengerBasket}");
                }

                //Validate Flight price with offer price data
                softAssertPassenger.True(passengerBasket.data.updatePeople.summary.flightPrice.Value != null, $"Passenger Validation : flightPrice field is null  \n Response : {passengerBasket}");

                softAssertPassenger.AssertAll();

                Assert.True(GraphQLAssertion.ValidatePassengerBasketMembershipDetails(passengerBasket, basketId, passengerId, out message), $"Basket Validation failed in passegner : \n{message}");

                passengerIndex++;
            }
        }

        private static DateTime ResolveLatestTravelDate(dynamic basket)
        {
            JObject basketResult = JObject.Parse(basket.ToString());
            List<DateTime> travelDates = basketResult
                .SelectTokens("$..sectors[*].departingAt")
                .Concat(basketResult.SelectTokens("$..departureDate"))
                .Concat(basketResult.SelectTokens("$..departing"))
                .Select(dateToken => dateToken?.ToString())
                .Where(dateValue => !dateValue.IsNullOrWhiteSpace())
                .Select(dateValue => DateTime.TryParse(dateValue, out DateTime parsedDate) ? parsedDate.Date : DateTime.MinValue)
                .Where(parsedDate => parsedDate != DateTime.MinValue)
                .ToList();

            if (travelDates.Count == 0)
            {
                return DateTime.Today;
            }

            return travelDates.Max();
        }

        public static string UpdatePassengerRequestGQL(string basketId, string passengerId, string firstName, string lastName, string dob, string gender, string type, string fqtProgramme = "", string fqtNumber = "", bool isPassengerProfile = false)
        {
            PassengerBasketGQLRequest updatePassengerReq = new PassengerBasketGQLRequest()
            {
                pqId = "updatePassengerMuationReqPqId",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.PassengerBasket.Variables()
                {
                    updatePeopleRequest = new UpdatePeopleRequest()
                    {
                        basketId = basketId,
                        people = new People()
                        {
                            id = passengerId,
                            firstName = firstName,
                            lastName = lastName,
                            dateOfBirth = dob,
                            gender = gender,
                            profilePassenger = isPassengerProfile,
                            validation = "VALID",
                            type = type,
                            membershipDetails = new MembershipDetails()
                            {
                                fqtProgramme = fqtProgramme,
                                fqtNumber = fqtNumber
                            }
                        }
                    }
                }
            };
            return JsonConvert.SerializeObject(updatePassengerReq);
        }

        public static void UpdateFCPassengerRequestGQL(string basketId, string fcProgram, string fcNumber, string dob, string fcFirstName, string fcLastName)
        {
            string gender = "Male";
            string type = "ADULT";

            var updatePassengerRequest = UpdatePassengerRequestGQL(basketId, "0", fcFirstName, fcLastName, dob, gender, type, fcProgram, fcNumber);

            var updatePassengerResponseGQL = GraphQLEndpoint.Book(updatePassengerRequest).EnsureGQLResponseSuccess();

            dynamic passengerBasket = JsonConvert.DeserializeObject<dynamic>(updatePassengerResponseGQL.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string passengerId = passengerBasket.data.updatePeople.people[0].id;
        }

        public static string RetrievePaymentGQLRequest(string basketId, string payWithPointsValue = null, double amountToPay = 0.0)
        {
            string paymentBaseUrl = ResolvePaymentRedirectBaseUrl();

            PaymentRetrieveGQLRequest paymentRetrieveGQLRequest = new()
            {
                pqId = "orderPaymentReferenceId",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.PaymentRetrieve.Variables()
                {
                    request = new Models.OrderCreateBookflowBFF.GraphQL.PaymentRetrieve.Request()
                    {
                        basketId = basketId,
                        amountToPay = amountToPay,
                        deviceInfo = null,
                        ipAddress = "10.229.65.4",
                        browserInfo = "PostmanRuntime/7.29.2",
                        paymentSuccessLink = $"{paymentBaseUrl}/payment/captured",
                        paymentFailureLink = $"{paymentBaseUrl}/payment/Failure",
                        payWithPointsValue = payWithPointsValue
                    }
                }
            };

            return JsonConvert.SerializeObject(paymentRetrieveGQLRequest);
        }

        private static string ResolvePaymentRedirectBaseUrl()
        {
            string environment = Config.Environment?.Trim() ?? string.Empty;
            if (environment.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("Unable to build payment redirect URL because Config.Environment is empty. Set the 'env' configuration value before running Book payment tests.");
            }

            if (environment.Is("prod"))
            {
                return "https://www.virginatlantic.com";
            }

            string redirectEnvironment = environment.Is("uat") ? "stg" : environment.ToLowerInvariant();
            return $"https://{redirectEnvironment}.virginatlantic.com";
        }

        public static string PaymentStatusGQLRequestBuild(string basketId, string pri, string payment = "")
        {
            PaymentStatusGQLRequest paymentStatusGQLRequest = new()
            {
                pqId = "bookUpdatePaymentStatusPqId",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.PaymentStatus.Variables()
                {
                    request = new Models.OrderCreateBookflowBFF.GraphQL.PaymentStatus.Request()
                    {
                        basketId = basketId,
                        paymentReferenceId = payment == "PayPal" ? pri : "",
                        hppPaymentStatus = "SUCCESS",
                    }
                }
            };
            return JsonConvert.SerializeObject(paymentStatusGQLRequest);
        }

        public static string OrderCreateGQLRequestBuild(string basketId, bool ccCheck = false)
        {
            OrderCreateGQLRequest orderCreateGQLRequest = new()
            {
                pqId = "bookOrderCreate",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.OrderConfirmation.Variables()
                {
                    request = new Models.OrderCreateBookflowBFF.GraphQL.OrderConfirmation.Request()
                    {
                        basketId = basketId,
                        paymentStatus = "Authorized",
                        deviceInfo = null,
                        ipAddress = "10.229.65.4",
                        browserInfo = "PostmanRuntime/7.29.2",
                        ccCheckConsent = ccCheck
                    }
                }
            };
            return JsonConvert.SerializeObject(orderCreateGQLRequest);
        }

        public static string RetrieveSeatMapGQLRequest(string basketId, string sectorId)
        {
            SeatGQLRequest seatGQLRequest = new()
            {
                pqId = "retrieveSeatMap",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.SeatGQL.RetrieveSeatMap.Variables()
                {
                    request = new Models.OrderCreateBookflowBFF.GraphQL.SeatGQL.RetrieveSeatMap.Request()
                    {
                        basketId = basketId,
                        sectorId = sectorId
                    }
                }
            };
            return JsonConvert.SerializeObject(seatGQLRequest);
        }

        public static string SeatSelectionGQLRequest(string basketId, string sectorId, string seatOfferId, string passengerId, string seatNumber, double amount, string currency)
        {
            SelectionSeatGQLRequest selectionSeatGQLRequest = new()
            {
                pqId = "selectSeat",
                variables = new Models.OrderCreateBookflowBFF.GraphQL.SeatGQL.SelectionSeat.Variables()
                {
                    request = new Models.OrderCreateBookflowBFF.GraphQL.SeatGQL.SelectionSeat.Request()
                    {
                        basketId = basketId,
                        flightPassengerSeatMap = new FlightPassengerSeatMap()
                        {
                            sectorId = sectorId,
                            seatOfferId = seatOfferId,
                            passengerId = passengerId,
                            seatNumber = seatNumber,
                            price = new Price()
                            {
                                amount = amount,
                                currency = currency
                            }
                        }
                    }
                }
            };
            return JsonConvert.SerializeObject(selectionSeatGQLRequest);
        }

        public static string PointToCurrencyConverter(string basketId)
        {
            var payload = File.ReadAllText(Path.Combine("requests", "Body", "GraphQl", "PayWithPoints", "PointsToCurrencyConverter.json"));

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            var variables = payloadObject["variables"];

            variables["basketId"] = basketId;
            var requestJson = JsonConvert.SerializeObject(payloadObject);

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string InsuranceQuote(string basketId)
        {
            var payload = File.ReadAllText(Path.Combine("requests", "Body", "GraphQl", "InsuranceQuote.json"));
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            var variables = payloadObject["variables"];
            variables["basketId"] = basketId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string InsuranceDecision(string basketId, bool isInsuranceSelected)
        {
            var payload = File.ReadAllText(Path.Combine("requests", "Body", "GraphQl", "InsuranceDecision.json"));
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            var variables = payloadObject["variables"];
            variables["request"]["basketId"] = basketId;
            variables["request"]["insuranceSelected"] = isInsuranceSelected;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SynchronyToken(string basketId)
        {
            var payload = File.ReadAllText(Path.Combine("requests", "Body", "GraphQl", "CoBrand", "SynchronyToken.json"));
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            var variables = payloadObject["variables"];
            variables["basketId"] = basketId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string RetrieveCoBrandApplicationStatus(string basketId, string synchronyToken)
        {
            var payload = File.ReadAllText(Path.Combine("requests", "Body", "GraphQl", "CoBrand", "RetrieveCoBrandApplicationStatus.json"));
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            var variables = payloadObject["variables"];
            variables["request"]["basketId"] = basketId;
            variables["request"]["synchronyToken"] = synchronyToken;

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}
