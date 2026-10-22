using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.OCI.Models.GraphQL.ContactTracing;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport;
using VAA.EAPI.Automation.OCI.Models.Microservices.CheckIn;
using VAA.EAPI.Automation.OCI.Models.Microservices.CheckPayment;
using VAA.EAPI.Automation.OCI.Models.Microservices.SaveBaggage;
using VAA.EAPI.Automation.OCI.Models.Microservices.SaveContactTracing;
using VAA.EAPI.Automation.OCI.Models.Microservices.SaveEmergencyContacts;
using VAA.EAPI.Automation.OCI.Models.Microservices.SavePassport;
using VAA.EAPI.Automation.OCI.Models.Microservices.SelectPassengers;
using VAA.EAPI.Automation.OCI.Models.Microservices.TravelRequirements;

namespace VAA.EAPI.Automation.OCI.Tests.Microservices
{
    public static class MicroserviceRequestBuilder
    {
        public static string CheckInMSRequest(string pnrLocator, string givenNames, string surname, string originCode, string eTicketNumber = "")
        {
            CheckInMSRequest checkInRequest = new()
            {
                PNRLocator = pnrLocator,
                ETicketNumber = eTicketNumber,
                GivenNames = givenNames,
                Surname = surname,
                OriginCode = originCode
            };

            return JsonConvert.SerializeObject(checkInRequest);
        }

        public static string SelectPassengersMSRequest(string travelPartyId, string firstName, string lastName, string pnrLocator, string originCode, List<string> selectedPassengerIds)
        {
            SelectPassengersMSRequest selectPassengersMSRequest = new()
            {
                TravelPartyId = travelPartyId,
                SelectedPassengerIds = selectedPassengerIds,
                FirstName = firstName,
                LastName = lastName,
                PnrLocator = pnrLocator,
                OriginCode = originCode
            };

            return JsonConvert.SerializeObject(selectPassengersMSRequest);
        }

        public static string SavePassportMSRequest(string travelPartyId, string requirementsId, string passengerId, string givenNames, string surname, string dateOfBirth)
        {
            Document document = new()
            {
                documentType = "PASSPORT",
                givenNames = givenNames,
                surname = surname,
                dateOfBirth = dateOfBirth,
                gender = new Gender()
                {
                    code = DataGenerator.GetRandomGenderCode()
                },
                documentNumber = DataGenerator.RandomNumber(trimLeadingZeros: true),
                expirationDate = DataGenerator.GetRandomFuturePassportExpiryDate(),
                issuingCountry = new IssuingCountry()
                {
                    code = "GBR"
                },
                nationality = new Nationality()
                {
                    code = "GBR"
                },
                countryOfResidence = new CountryOfResidence()
                {
                    code = "GBR"
                }
            };

            SavePassportMSRequest savePassportMSRequest = new()
            {
                TravelPartyId = travelPartyId,
                RequirementsId = requirementsId,
                PassengerId = passengerId,
                Document = document,
                IsScanned = false,
                MRZLine1 = "",
                MRZLine2 = ""
            };

            return JsonConvert.SerializeObject(savePassportMSRequest);
        }

        public static string SaveContactTracingMSRequest(string travelPartyId, string requirementsId, List<string> selectedPassengerIds, bool applyToAllPassengers = false)
        {
            List<ContactTracingInfo> contactTracings = new();

            foreach (string passengerId in selectedPassengerIds)
            {
                ContactTracingInfo contactTracing = new()
                {
                    passengerId = passengerId,
                    streetAddress = "456 Park Ave",
                    city = "New York",
                    state = "NY",
                    zip = "10022",
                    email = DataGenerator.GenerateRandomEmail(),
                    inTransit = false,
                    noEmail = true,
                    noSecondaryPhoneNumber = false,
                    primaryPhoneNumber = DataGenerator.RandomNumber(),
                    primaryPhoneNumberCountryCode = DataGenerator.RandomPhoneCountryCode(),
                    secondaryPhoneNumber = DataGenerator.RandomNumber(),
                    secondaryPhoneNumberCountryCode = DataGenerator.RandomPhoneCountryCode()
                };

                contactTracings.Add(contactTracing);

                if (applyToAllPassengers)
                    break; // Exit after first passenger if applyToAll is true
            }
            ;

            SaveContactTracingRequest saveContactTracingRequest = new()
            {
                TravelPartyId = travelPartyId,
                RequirementsId = requirementsId,
                ApplyToAllPassengers = applyToAllPassengers,
                ContactTracings = contactTracings
            };

            return JsonConvert.SerializeObject(saveContactTracingRequest);
        }

        public static string SaveEmergencyContactMSRequest(HttpResponseMessage jsonResponse, string travelPartyId, string requirementsId, bool applyToAllPassengers = false)
        {
            JToken checkInResult = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Select all passengers
            IEnumerable<JToken> passengers = checkInResult.SelectTokens("$.passengers[*]");

            List<EmergencyContact> contacts = new List<EmergencyContact>();

            foreach (JToken p in passengers)
            {
                EmergencyContact contact = new()
                {
                    passengerId = p.SelectToken("passengerId")?.ToString(),
                    firstname = p.SelectToken("givenNames")?.ToString(),
                    lastname = p.SelectToken("surname")?.ToString(),
                    phoneContryCode = DataGenerator.RandomPhoneCountryCode(),
                    phoneNumber = DataGenerator.RandomNumber(),
                    passengerRefused = false
                };

                contacts.Add(contact);
            }

            EmergencyContactRequest emergencyContactRequest = new()
            {
                TravelPartyId = travelPartyId,
                RequirementsId = requirementsId,
                ApplyToAllPassengers = applyToAllPassengers,
                EmergencyContacts = contacts
            };

            return JsonConvert.SerializeObject(emergencyContactRequest);
        }

        public static string SaveBaggageMSRequest(HttpResponseMessage jsonResponse, string travelPartyId)
        {
            JToken baggageJson = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            SaveBaggageMSRequest saveBaggageMSRequest = new()
            {
                travelPartyId = travelPartyId,
                BaggagePricing = new List<BaggagePricing>()
            };

            // Loop through all passengers
            foreach (JToken passenger in baggageJson["passengers"])
            {
                string passengerNiN = passenger["passengerNiN"]?.ToString();

                // Loop through baggage offers for this passenger
                foreach (JToken offer in passenger["baggageOffers"])
                {
                    saveBaggageMSRequest.BaggagePricing.Add(new BaggagePricing
                    {
                        passengerNiN = passengerNiN,
                        baggageOfferId = offer["baggageOfferId"]?.ToString(),
                        baggageCount = offer["baggageCount"]?.ToObject<int>() ?? 0,
                        totalPrice = offer["price"]?["amount"]?.ToObject<double>() ?? 0.0
                    });
                }
            }

            return JsonConvert.SerializeObject(saveBaggageMSRequest);
        }

        public static string CreateBaggageBasketRequest(HttpResponseMessage jsonResponse)
        {
            JToken baggageJson = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            CheckedBaggageRequest checkedBaggageRequest = new()
            {
                BasketId = Guid.NewGuid().ToString(),
                CheckedBaggage = new List<CheckedBaggageItem>()
            };

            // Loop through all passengers
            foreach (JToken passenger in baggageJson["passengers"])
            {
                string passengerNiN = passenger["passengerNiN"]?.ToString();

                // Loop through baggage offers for this passenger
                foreach (JToken offer in passenger["baggageOffers"])
                {
                    checkedBaggageRequest.CheckedBaggage.Add(new CheckedBaggageItem
                    {
                        PassengerNiN = passengerNiN,
                        BaggageOfferId = offer["baggageOfferId"]?.ToString(),
                        BaggageCount = offer["baggageCount"]?.ToObject<int>() ?? 0,
                        TotalPrice = offer["price"]?["amount"]?.ToObject<double>() ?? 0.0
                    });
                }
            }

            return JsonConvert.SerializeObject(checkedBaggageRequest);
        }

        public static string CheckPaymentMSRequest(string travelPartyId)
        {
            CheckPaymentMSRequest checkPaymentMSRequest = new()
            {
                travelPartyId = travelPartyId,
                paymentPending = false
            };

            return JsonConvert.SerializeObject(checkPaymentMSRequest);
        }

        public static string TravelRequirementsMSRequest(string pnrLocator, string givenNames, string surname)
        {
            TravelRequirementsMSRequest travelRequirementsMSRequest = new()
            {
                PNRLocator = pnrLocator,
                GivenNames = givenNames,
                Surname = surname
            };

            return JsonConvert.SerializeObject(travelRequirementsMSRequest);
        }
    }
}
