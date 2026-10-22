using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.OCI.Models.GraphQL.CheckIn;
using VAA.EAPI.Automation.OCI.Models.GraphQL.ConfirmBaggage;
using VAA.EAPI.Automation.OCI.Models.GraphQL.ContactTracing;
using VAA.EAPI.Automation.OCI.Models.GraphQL.GenerateBoardingPass;
using VAA.EAPI.Automation.OCI.Models.GraphQL.GetBaggage;
using VAA.EAPI.Automation.OCI.Models.GraphQL.PaymentEligibilityCheck;
using VAA.EAPI.Automation.OCI.Models.GraphQL.RetrieveBoardingPass;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SaveBaggage;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SaveEmergencyContacts;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SaveReturnDate;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SelectPassengers;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SecureFlightInfo;
using VAA.EAPI.Automation.OCI.Models.GraphQL.TravelRequirements;

namespace VAA.EAPI.Automation.OCI.Tests.GraphQL
{
    public static class GraphQLRequestBuilder
    {
        public static string CheckInGraphQLRequest(string pnrLocator, string givenNames, string surname, string originCode, string eTicketNumber = "")
        {
            string checkInGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "CheckInGraphQLQuery.txt"));

            CheckInGraphQLRequest checkInGraphQLRequest = new()
            {
                query = checkInGraphQLQuery,
                variables = new CheckInVariables()
                {
                    pnrLocator = pnrLocator,
                    eTicketNumber = eTicketNumber,
                    givenNames = givenNames,
                    surname = surname,
                    originCode = originCode
                }
            };

            return JsonConvert.SerializeObject(checkInGraphQLRequest);
        }

        public static string SelectPassengersGraphQLRequest(string checkInId, List<string> passengerIds)
        {
            string selectPassengersGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SelectPassengersGraphQLQuery.txt"));

            SelectPassengersGraphQLRequest selectPassengersGraphQLRequest = new()
            {
                query = selectPassengersGraphQLQuery,
                variables = new SelectPassengersVariables()
                {
                    checkInId = checkInId,
                    passengerIds = passengerIds
                }
            };

            return JsonConvert.SerializeObject(selectPassengersGraphQLRequest);
        }

        public static string SavePassportGraphQLRequest(string checkInId, string passengerId, string givenNames, string surname, string dateOfBirth, string documentNumber, string passportIssuingCountry = "GBR", string nationality = "GBR", string countryOfResidence = "GBR", bool isScanned = false)
        {
            string savePassportGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SavePassportGraphQLQuery.txt"));

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
                documentNumber = documentNumber,
                expirationDate = DataGenerator.GetRandomFuturePassportExpiryDate(),
                issuingCountry = new IssuingCountry()
                {
                    code = passportIssuingCountry
                },
                nationality = new Nationality()
                {
                    code = nationality
                },
                countryOfResidence = new CountryOfResidence()
                {
                    code = countryOfResidence
                }
            };

            SavePassportGraphQLRequest savePassportGraphQLRequest = new()
            {
                query = savePassportGraphQLQuery,
                variables = new SavePassportVariables()
                {
                    checkInId = checkInId,
                    passengerId = passengerId,
                    document = document,
                    isScanned = isScanned,
                    mRZLine1 = "",
                    mRZLine2 = ""
                }
            };

            return JsonConvert.SerializeObject(savePassportGraphQLRequest);
        }

        public static string SaveContactTracingGraphQLRequest(string checkInId, List<string> selectedPassengerIds, bool applyToAll = false)
        {
            string saveContactTracingGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SaveContactTracingGraphQLQuery.txt"));

            List<ContactTracingInfo> contactTracings = new List<ContactTracingInfo>();

            // All the hardcoded values below will be removed once API integration done with delta API.
            foreach (string passengerId in selectedPassengerIds)
            {
                ContactTracingInfo contactTracing = new ContactTracingInfo
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

                if (applyToAll)
                    break; // Exit after first passenger if applyToAll is true
            }

            ContactTracingGraphQLRequest contactTracingGraphQLRequest = new()
            {
                query = saveContactTracingGraphQLQuery,
                variables = new ContactTracingVariables()
                {
                    checkInId = checkInId,
                    applyToAll = applyToAll,
                    contactTracings = contactTracings
                }
            };

            return JsonConvert.SerializeObject(contactTracingGraphQLRequest);
        }

        public static string ReferenceGraphQLRequest(string referenceCall)
        {
            string fileName = referenceCall switch
            {
                "GetGenderDetails" => "GetGenderDetails.json",
                "GetStatesOfUSA" => "GetStatesOfUSA.json",
                "GetNationalityDetails" => "GetNationalityDetails.json",
                "GetPassportCountries" => "GetPassportCountries.json",
                _ => throw new ArgumentException($"Invalid reference call: {referenceCall}")
            };

            string filePath = Path.Combine("Requests", "Body", "GraphQL", fileName);
            return File.ReadAllText(filePath);
        }

        public static string GetTravelRequirementsGraphQLRequest(string pnrLocator, string givenNames, string surname)
        {
            string getTravelRequirementsQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "GetTravelRequirementsGraphQLQuery.txt"));

            GetTravelRequirementsGraphQLRequest request = new()
            {
                query = getTravelRequirementsQuery,
                variables = new TravelRequirementsVariables()
                {
                    request = new TravelRequirementsRequestInput()
                    {
                        pnrLocator = pnrLocator,
                        givenNames = givenNames,
                        surname = surname
                    }
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        public static string SavePassportTravelRequirementsGraphQLRequest(
            string travelRequirementsId,
            string passengerId,
            string givenNames,
            string surname,
            string dateOfBirth,
            string documentNumber,
            string passportIssuingCountry = "GBR",
            string nationality = "GBR",
            string countryOfResidence = "GBR",
            bool isScanned = false)
        {
            string savePassportTravelRequirementsQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SavePassportTravelRequirementsGraphQLQuery.txt"));

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
                documentNumber = documentNumber,
                expirationDate = DataGenerator.GetRandomFuturePassportExpiryDate(),
                issuingCountry = new IssuingCountry()
                {
                    code = passportIssuingCountry
                },
                nationality = new Nationality()
                {
                    code = nationality
                },
                countryOfResidence = new CountryOfResidence()
                {
                    code = countryOfResidence
                }
            };

            SavePassportTravelRequirementsGraphQLRequest request = new()
            {
                query = savePassportTravelRequirementsQuery,
                variables = new SavePassportTravelRequirementsVariables()
                {
                    request = new SavePassportTravelRequirementsRequestInput()
                    {
                        travelRequirementsId = travelRequirementsId,
                        passengerId = passengerId,
                        isScanned = isScanned,
                        document = document
                    }
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        public static string SaveSecondaryDocumentTravelRequirementsGraphQLRequest(string travelRequirementsId, string passengerId, string sliceId, string secondaryDocumentId, string countryCode)
        {
            string saveSecondaryDocumentTravelRequirementsQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SaveSecondaryDocumentTravelRequirementsGraphQLQuery.txt"));

            SaveSecondaryDocumentTravelRequirementsGraphQLRequest request = new()
            {
                query = saveSecondaryDocumentTravelRequirementsQuery,
                variables = new SaveSecondaryDocumentTravelRequirementsVariables()
                {
                    travelRequirementsId = travelRequirementsId,
                    passengerId = passengerId,
                    sliceId = sliceId,
                    secondaryDocumentId = secondaryDocumentId,
                    countryCode = countryCode
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        public static string SaveEmergencyContactsGraphQLRequest(JToken jsonResponse, string checkInId, bool applyToAllPassengers = false)
        {
            string saveEmergencyContactsGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SaveEmergencyContactsGraphQLQuery.txt"));

            // Select all passengers
            IEnumerable<JToken> passengers = jsonResponse.SelectTokens("$..passengers[*]");

            List<Contact> contacts = new List<Contact>();

            foreach (JToken p in passengers)
            {
                Contact contact = new Contact
                {
                    passengerId = p.SelectToken("passengerId")?.ToString(),
                    firstName = p.SelectToken("givenNames")?.ToString(),
                    lastName = p.SelectToken("surname")?.ToString(),
                    phoneCountryCode = DataGenerator.RandomISO2CountryCode(),
                    phoneNumber = DataGenerator.RandomNumber(),
                    passengerRefused = false
                };

                contacts.Add(contact);

                if (applyToAllPassengers)
                    break; // Exit after first passenger if applyToAllPassengers is true
            }

            SaveEmergencyContactsGraphQLRequest saveEmergencyContactsGraphQLRequest = new()
            {
                query = saveEmergencyContactsGraphQLQuery,
                variables = new SaveEmergencyContactsVariables()
                {
                    checkInId = checkInId,
                    applyToAllPassengers = applyToAllPassengers,
                    emergencyContacts = contacts
                }
            };

            return JsonConvert.SerializeObject(saveEmergencyContactsGraphQLRequest);
        }

        public static string SaveReturnDateGraphQLRequest(string checkInId, string passengerId, string returnOn, bool noReturnDate = false)
        {
            string saveReturnDateGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SaveReturnDateGraphQLQuery.txt"));

            SaveReturnDateGraphQLRequest saveReturnDateGraphQLRequest = new()
            {
                query = saveReturnDateGraphQLQuery,
                variables = new SaveReturnDateVariables()
                {
                    checkInId = checkInId,
                    passengerId = passengerId,
                    returnOn = returnOn,
                    noReturnDate = noReturnDate
                }
            };

            return JsonConvert.SerializeObject(saveReturnDateGraphQLRequest);
        }

        public static string GenerateBoardingPassGraphQLRequest(string checkInId)
        {
            string generateBoardingPassGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "GenerateBoardingPassGraphQLQuery.txt"));

            GenerateBoardingPassGraphQLRequest generateBoardingPassGraphQLRequest = new()
            {
                query = generateBoardingPassGraphQLQuery,
                variables = new GenerateBoardingPassVariables()
                {
                    checkInId = checkInId
                }
            };

            return JsonConvert.SerializeObject(generateBoardingPassGraphQLRequest);
        }

        public static string RetrieveBoardingPassGraphQLRequest(string pnrLocator, string originCode, string givenNames, string surname)
        {
            string retrieveBoardingPassGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "RetrieveBoardingPassGraphQLQuery.txt"));

            RetrieveBoardingPassGraphQLRequest retrieveBoardingPassGraphQLRequest = new()
            {
                query = retrieveBoardingPassGraphQLQuery,
                variables = new RetrieveBoardingPassVariables()
                {
                    pnrLocator = pnrLocator,
                    originCode = originCode,
                    givenNames = givenNames,
                    surname = surname
                }
            };

            return JsonConvert.SerializeObject(retrieveBoardingPassGraphQLRequest);
        }

        public static string GetSecureFlightInfoGraphQLRequest(
            string pnrLocator,
            string givenNames,
            string surname,
            string passengerFirstNin,
            string passengerLastNin)
        {
            string getSecureFlightInfoGraphQLQuery = File.ReadAllText(
                Path.Combine("Requests", "Body", "GraphQL", "GetSecureFlightInfoGraphQLQuery.txt"));

            GetSecureFlightInfoGraphQLRequest getSecureFlightInfoGraphQLRequest = new()
            {
                query = getSecureFlightInfoGraphQLQuery,
                variables = new SecureFlightInfoVariables
                {
                    request = new SecureFlightInfoRequestInput
                    {
                        pnrLocator = pnrLocator,
                        givenNames = givenNames,
                        surname = surname,
                        passengerFirstNin = passengerFirstNin,
                        passengerLastNin = passengerLastNin
                    }
                }
            };

            return JsonConvert.SerializeObject(getSecureFlightInfoGraphQLRequest);
        }

        public static string UpdateSecureFlightInfoGraphQLRequest(
            string secureFlightReservationId,
            string passengerId,
            string passengerFirstNin,
            string passengerLastNin,
            List<SecureFlightDetailsInput> secureFlightDetails)
        {
            string updateSecureFlightInfoGraphQLQuery = File.ReadAllText(
                Path.Combine("Requests", "Body", "GraphQL", "UpdateSecureFlightInfoGraphQLQuery.txt"));

            UpdateSecureFlightInfoGraphQLRequest updateSecureFlightInfoGraphQLRequest = new()
            {
                query = updateSecureFlightInfoGraphQLQuery,
                variables = new UpdateSecureFlightInfoVariables
                {
                    request = new UpdateSecureFlightInfoRequestInput
                    {
                        secureFlightReservationId = secureFlightReservationId,
                        passengerId = passengerId,
                        passengerFirstNin = passengerFirstNin,
                        passengerLastNin = passengerLastNin,
                        secureFlightDetails = secureFlightDetails
                    }
                }
            };

            return JsonConvert.SerializeObject(updateSecureFlightInfoGraphQLRequest);
        }

        public static string GetBaggageGraphQLRequest(string checkInId, bool hidePaidBaggage)
        {
            string getBaggageGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "GetBaggageDetailsGraphQLQuery.txt"));

            GetBaggageGraphQLRequest getBaggageGraphQLRequest = new()
            {
                query = getBaggageGraphQLQuery,
                variables = new GetBaggageVariables()
                {
                    checkInId = checkInId,
                    hidePaidBaggage = hidePaidBaggage
                }
            };

            return JsonConvert.SerializeObject(getBaggageGraphQLRequest);
        }

        public static string SaveBaggageGraphQLRequest(HttpResponseMessage jsonResponse, string checkInId, BaggageSelectionType selectionType, int baggageCount = 3)
        {
            JToken baggageJson = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Models.GraphQL.SaveBaggage.Request request = new()
            {
                checkInId = checkInId,
                selectedBaggageOffer = new List<SelectedBaggageOffer>()
            };

            JArray passengers = baggageJson["data"]?["BaggageAllowance"]?["passengers"] as JArray;

            // Loop through all passengers
            foreach (JToken passenger in passengers)
            {
                string passengerNiN = passenger["passengerNiN"]?.ToString();
                bool isInfantPassenger = passenger["isInfantPassenger"]?.Value<bool>() ?? false;
                JArray baggageOffers = passenger["baggageOffers"] as JArray;

                if (passengerNiN.IsNullOrWhiteSpace() || baggageOffers == null || isInfantPassenger)
                    continue;

                JToken? freeOffer = baggageOffers
                    .Where(o => (decimal?)o["price"]?["amount"] == 0)
                    .LastOrDefault();

                JToken? paidOffer = baggageOffers.FirstOrDefault(o => ((decimal?)o["price"]?["amount"] ?? 0) > 0 && ((int?)o["baggageCount"] ?? 0) == baggageCount);

                if ((selectionType == BaggageSelectionType.PaidOnly || selectionType == BaggageSelectionType.FreePlusPaid) && paidOffer == null)
                {
                    IEnumerable<int> availablePaidCounts = baggageOffers
                        .Where(o => ((decimal?)o["price"]?["amount"] ?? 0) > 0)
                        .Select(o => (int?)o["baggageCount"])
                        .Where(c => c.HasValue)
                        .Distinct()
                        .OrderBy(c => c)
                        .Select(c => c.Value);

                    throw new InvalidOperationException(
                        $"No paid baggage offer found for passenger {passengerNiN} with baggageCount={baggageCount}. Available paid baggageCounts: {string.Join(", ", availablePaidCounts)}");
                }

                void AddOffer(JToken offer)
                {
                    request.selectedBaggageOffer.Add(new SelectedBaggageOffer
                    {
                        passengerNiN = passengerNiN,
                        baggageOfferId = offer["baggageOfferId"]?.ToString(),
                        baggageCount = offer["baggageCount"]?.Value<int>() ?? 0,
                        totalPrice = offer["price"]?["amount"]?.Value<decimal>() ?? 0.0m,
                        currencyCode = offer["price"]?["currency"]?.ToString(),
                        decimalPrecisionCount = offer["price"]?["decimalPrecisionCount"]?.Value<int>() ?? 0
                    });
                }

                switch (selectionType)
                {
                    case BaggageSelectionType.FreeOnly:
                        if (freeOffer != null)
                            AddOffer(freeOffer);
                        break;

                    case BaggageSelectionType.PaidOnly:
                        if (paidOffer != null)
                            AddOffer(paidOffer);
                        break;

                    case BaggageSelectionType.FreePlusPaid:
                        if (freeOffer != null)
                            AddOffer(freeOffer);
                        if (paidOffer != null)
                            AddOffer(paidOffer);
                        break;
                }
            }

            string saveBaggageGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SaveBaggageDetailsGraphQLQuery.txt"));

            SaveBaggageGraphQLRequest saveBaggageGraphQLRequest = new()
            {
                query = saveBaggageGraphQLQuery,
                variables = new SaveBaggageVariables()
                {
                    request = request
                }
            };

            return JsonConvert.SerializeObject(saveBaggageGraphQLRequest);
        }

        public static string PaymentEligibilityCheckGraphQLRequest(string checkInId)
        {
            string paymentEligibilityCheckGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "PaymentEligibilityCheckGraphQLQuery.txt"));

            PaymentEligibilityCheckGraphQLRequest paymentEligibilityCheckGraphQLRequest = new()
            {
                query = paymentEligibilityCheckGraphQLQuery,
                variables = new PaymentEligibilityCheckVariables()
                {
                    request = new Models.GraphQL.PaymentEligibilityCheck.Request()
                    {
                        checkInId = checkInId,
                        //These urls might change or not needed and development for paid bags still in progress until delta team fix the issue.just added to test the flow.
                        successRedirectUrl = "https://travelplus-test2.webdev.vholsinternal.co.uk/flight-only/payment/captured",
                        failureRedirectUrl = "https://travelplus-test2.webdev.vholsinternal.co.uk/flight-only/payment/failed"
                    }
                }
            };

            return JsonConvert.SerializeObject(paymentEligibilityCheckGraphQLRequest);
        }

        public static string ConfirmBaggageGraphQLRequest(string checkInId, string paymentReferenceId)
        {
            string confirmBaggageGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "ConfirmBaggageGraphQLQuery.txt"));

            ConfirmBaggageGraphQLRequest confirmBaggageGraphQLRequest = new()
            {
                query = confirmBaggageGraphQLQuery,
                variables = new ConfirmBaggageVariables()
                {
                    confirmBaggageRequest = new ConfirmBaggageRequest()
                    {
                        checkInId = checkInId,
                        paymentReferenceId = paymentReferenceId,
                        paymentStatus = "SUCCESS"
                    }
                }
            };

            return JsonConvert.SerializeObject(confirmBaggageGraphQLRequest);
        }

        public static string TravelRequirementsSaveReturnDateGraphQLRequest(string passengerId, string travelRequirementsId, string returnOn, bool noReturnDate = false)
        {
            string travelRequirementsSaveReturnDateQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "TravelRequirementsSaveReturnDateGraphQLQuery.txt"));

            TravelRequirementsSaveReturnDateGraphQLRequest request = new()
            {
                query = travelRequirementsSaveReturnDateQuery,
                variables = new TravelRequirementsSaveReturnDateVariables()
                {
                    request = new TravelRequirementsSaveReturnDateRequestInput()
                    {
                        passengerId = passengerId,
                        travelRequirementsId = travelRequirementsId,
                        returnOn = returnOn,
                        noReturnDate = noReturnDate
                    }
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        public static string TravelRequirementsSaveContactTracingGraphQLRequest(string travelRequirementsId, JToken response)
        {
            string query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "TravelRequirementsSaveContactTracingGraphQLQuery.txt"));

            JArray? passengers = response["data"]?["SaveEmergencyContactTravelRequirements"]?["passengers"] as JArray;

            Assert.NotNull(passengers);

            string? targetSliceId = null;

            List<ContactTracingInfo> contactTracings = new List<ContactTracingInfo>();

            foreach (JToken passenger in passengers)
            {
                string? passengerId = passenger["passengerId"]?.ToString();

                if (passengerId.IsNullOrWhiteSpace())
                    continue;

                if (passenger["sliceRequirements"] is not JArray sliceRequirements)
                    continue;

                JToken? requirement = sliceRequirements.FirstOrDefault(r =>
                    (r["contactTracing"]?.ToString() ?? string.Empty).Is(CheckInStatus.TO_COMPLETE.ToString()) &&
                    !(r["sliceId"]?.ToString() ?? string.Empty).IsNullOrWhiteSpace());

                if (requirement == null)
                    continue;

                string requirementSliceId = requirement["sliceId"]!.ToString();

                targetSliceId ??= requirementSliceId;

                if (!string.Equals(targetSliceId, requirementSliceId, StringComparison.Ordinal))
                    continue;

                contactTracings.Add(CreateContactTracing(passengerId));
            }

            Assert.False(targetSliceId.IsNullOrWhiteSpace(), "No slice requiring contact tracing was found.");

            Assert.NotEmpty(contactTracings);

            TravelRequirementsSaveContactTracingGraphQLRequest request = new()
            {
                query = query,
                variables = new TravelRequirementsSaveContactTracingVariables
                {
                    travelRequirementsId = travelRequirementsId,
                    sliceId = targetSliceId,
                    contactTracing = contactTracings
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        private static ContactTracingInfo CreateContactTracing(string passengerId) =>
            new()
            {
                passengerId = passengerId,
                streetAddress = "456 Park Ave",
                city = "New York",
                state = "NY",
                zip = "10022",
                email = DataGenerator.GenerateRandomEmail(),
                inTransit = false,
                noEmail = false,
                noSecondaryPhoneNumber = false,
                primaryPhoneNumber = DataGenerator.RandomNumber(),
                primaryPhoneNumberCountryCode = DataGenerator.RandomPhoneCountryCode(),
                secondaryPhoneNumber = DataGenerator.RandomNumber(),
                secondaryPhoneNumberCountryCode = DataGenerator.RandomPhoneCountryCode()
            };

        public static string SaveEmergencyContactTravelRequirementsGraphQLRequest(JToken jsonResponse, string travelRequirementsId, bool applyToAllPassengers = false)
        {
            string saveEmergencyContactsTravelRequirementsGraphQLQuery = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SaveEmergencyContactTravelRequirementsGraphQLQuery.txt"));

            // Select all passengers
            IEnumerable<JToken> passengers = jsonResponse.SelectTokens("$..passengers[*]");

            List<Contact> contacts = new List<Contact>();

            foreach (JToken p in passengers)
            {
                Contact contact = new Contact
                {
                    passengerId = p.SelectToken("passengerId")?.ToString(),
                    firstName = p.SelectToken("givenNames")?.ToString(),
                    lastName = p.SelectToken("surname")?.ToString(),
                    phoneCountryCode = DataGenerator.RandomISO2CountryCode(),
                    phoneNumber = DataGenerator.RandomNumber(),
                    passengerRefused = false
                };

                contacts.Add(contact);

                if (applyToAllPassengers)
                    break; // Exit after first passenger if applyToAllPassengers is true
            }

            SaveEmergencyContactTravelRequirementsGraphQLRequest saveEmergencyContactTravelRequirementsGraphQLRequest = new()
            {
                query = saveEmergencyContactsTravelRequirementsGraphQLQuery,
                variables = new SaveEmergencyContactTravelRequirementsVariables()
                {
                    travelRequirementsId = travelRequirementsId,
                    applyToAllPassengers = applyToAllPassengers,
                    emergencyContacts = contacts
                }
            };

            return JsonConvert.SerializeObject(saveEmergencyContactTravelRequirementsGraphQLRequest);
        }

        public static string SaveGovernmentFormTravelRequirementsGraphQLRequest(string travelRequirementsId, string passengerId, string sliceId, string governmentFormId, string linkId,
            string text, string href)
        {
            string query = File.ReadAllText(Path.Combine("Requests", "Body", "GraphQL", "SaveGovernmentFormTravelRequirementsGraphQLQuery.txt"));

            SaveGovernmentFormTravelRequirementsGraphQLRequest request = new()
            {
                query = query,
                variables = new SaveGovernmentFormTravelRequirementsVariables
                {
                    travelRequirementsId = travelRequirementsId,
                    passengerId = passengerId,
                    sliceId = sliceId,
                    governmentFormId = governmentFormId,
                    linkId = linkId,
                    text = text,
                    href = href
                }
            };

            return JsonConvert.SerializeObject(request);
        }

        public static bool TryBuildSaveGovernmentFormTravelRequirementsGraphQLRequestFromPassportResponse(JToken savePassportResult, string travelRequirementsId, string passengerId,
            out string saveGovernmentFormRequestJson, out string sliceId, out string governmentFormId)
        {
            saveGovernmentFormRequestJson = string.Empty;
            sliceId = string.Empty;
            governmentFormId = string.Empty;

            IEnumerable<JToken> passengerSliceRequirementsFromPassportResponse = savePassportResult.SelectTokens(
                $"$.data.SavePassportTravelRequirements.passengers[?(@.passengerId == '{passengerId}')].sliceRequirements[*]");

            foreach (JToken sliceRequirement in passengerSliceRequirementsFromPassportResponse)
            {
                string sliceRequirementSliceId = sliceRequirement["sliceId"]?.ToString();

                if (sliceRequirementSliceId.IsNullOrWhiteSpace())
                {
                    continue;
                }

                if (sliceRequirement["governmentForms"] is not JArray governmentForms || governmentForms.Count == 0)
                {
                    continue;
                }

                foreach (JToken governmentForm in governmentForms)
                {
                    bool isAvailable = governmentForm["isAvailable"]?.Value<bool?>() ?? false;

                    if (!isAvailable)
                    {
                        continue;
                    }

                    string candidateGovernmentFormId = governmentForm["id"]?.ToString();
                    string linkId = governmentForm["linkId"]?.ToString();
                    string text = governmentForm["title"]?.ToString();
                    string href = governmentForm["url"]?.ToString();

                    if (candidateGovernmentFormId.IsNullOrWhiteSpace()
                        || linkId.IsNullOrWhiteSpace()
                        || text.IsNullOrWhiteSpace()
                        || href.IsNullOrWhiteSpace())
                    {
                        continue;
                    }

                    sliceId = sliceRequirementSliceId;
                    governmentFormId = candidateGovernmentFormId;

                    saveGovernmentFormRequestJson = SaveGovernmentFormTravelRequirementsGraphQLRequest(
                        travelRequirementsId,
                        passengerId,
                        sliceId,
                        governmentFormId,
                        linkId,
                        text,
                        href);

                    return true;
                }
            }

            return false;
        }
    }
}
