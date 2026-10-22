using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.OCI.Models.Delta;
using VAA.EAPI.Automation.OCI.Models.GraphQL.SavePassport;

namespace VAA.EAPI.Automation.OCI.Tests.Delta
{
    public static class DeltaMicroserviceRequestBuilder
    {
        public static string TravelPartiesRequest(string recordLocator, string originCode)
        {
            TravelPartyRequest travelPartyRequest = new()
            {
                recordLocator = recordLocator,
                originCode = originCode
            };

            return JsonConvert.SerializeObject(travelPartyRequest);
        }

        public static string SelectPassengersRequest(JToken travelPartiesResult)
        {
            var selectedPassengerList = travelPartiesResult?.SelectTokens("$..passengers").OfType<JArray>().SelectMany(p => p).Where(p => p != null).Select(p => new SelectedPassenger
            {
                id = (string)p["id"],
                customerId = (string)p["customerId"],
                firstNIN = (string)p["firstNIN"],
                lastNIN = (string)p["lastNIN"]
            }).ToList();

            SelectedPassengerRequest selectedPassengerRequest = new()
            {
                selectedPassenger = selectedPassengerList
            };

            return JsonConvert.SerializeObject(selectedPassengerRequest);
        }

        public static string SelectPassengerRequest(string passengerId, string customerId, string firstNin, string lastNin)
        {
            SelectedPassenger selectedPassenger = new()
            {
                id = passengerId,
                customerId = customerId,
                firstNIN = firstNin,
                lastNIN = lastNin
            };

            SelectedPassengerRequest selectedPassengerRequest = new()
            {
                selectedPassenger = new List<SelectedPassenger> { selectedPassenger }
            };

            return JsonConvert.SerializeObject(selectedPassengerRequest);
        }

        public static string SavePassportRequest(string givenNames, string surname, string birthDate, string passengerResourceId)
        {
            IdentityDocument identityDocument = new()
            {
                type = "PASSPORT",
                givenNames = givenNames,
                middleName = "",
                surname = surname,
                birthDate = birthDate,
                gender = new Gender()
                {
                    code = DataGenerator.GetRandomGenderCode()
                },
                number = DataGenerator.RandomNumber(trimLeadingZeros: true),
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
                },
                dataSourceType = "MANUAL"
            };

            Request request = new()
            {
                passengerResourceId = passengerResourceId,
                @using = "PASSPORT",
                identityDocument = identityDocument,
                consent = true,
                modified = true,
                saveToProfile = false
            };

            SavePassportDetailsRequest savePassportDetailsRequest = new()
            {
                requests = new List<Request> { request }
            };

            return JsonConvert.SerializeObject(savePassportDetailsRequest);
        }

        public static string RetrieveRequirements(string recordLocator, string givenNames, string surname)
        {
            RetrieveRequirementsRequest retrieveRequirementsRequest = new()
            {
                @using = "CONFIRMATION",
                confirmationNum = recordLocator,
                givenNames = givenNames,
                surname = surname
            };

            return JsonConvert.SerializeObject(retrieveRequirementsRequest);
        }

        public static string SaveEmergencyContactsRequest(string passengerResourceId, string firstName, string lastName)
        {
            var request = new EmergencyContactRequest.Request
            {
                passengerResourceId = passengerResourceId,
                @using = "EMERGENCY_CONTACT",
                emergencyContact = new List<EmergencyContactRequest.EmergencyContact>
                {
                    new EmergencyContactRequest.EmergencyContact
                    {
                     declined = false,
                     type = "PRIMARY",
                     countryCode = DataGenerator.RandomPhoneCountryCode(),
                     phoneNumber = DataGenerator.RandomNumber(),
                     firstName = firstName,
                     lastName = lastName
                    }
                }
            };

            EmergencyContactRequest emergencyContactRequest = new()
            {
                requests = new List<EmergencyContactRequest.Request> { request }
            };

            return JsonConvert.SerializeObject(emergencyContactRequest);
        }

        public static string SaveContactTracingRequest(string passengerResourceId, string tripId)
        {
            var request = new ContactTracingRequest.Request
            {
                passengerResourceId = passengerResourceId,
                tripId = tripId,
                @using = "CONTACT_TRACING",
                contactInformation = new List<ContactTracingRequest.ContactInformation>
                {
                    new ContactTracingRequest.ContactInformation
                    {
                        address = new ContactTracingRequest.Address
                        {
                          streetAddress = "123 Main St",
                          cityCode = "NYC",
                          stateCode = "NY",
                          countryCode = "US",
                          zipCode = "10001",
                          declined = false
                        },
                        email = new List<ContactTracingRequest.Email>
                        {
                            new ContactTracingRequest.Email
                            {
                                declined = false,
                                value = DataGenerator.GenerateRandomEmail()
                            }
                        },
                        phone = new List<ContactTracingRequest.Phone>
                        {
                            new ContactTracingRequest.Phone
                            {
                               declined = false,
                               type = "HOME",
                               preference = "PRIMARY",
                               countryCode = DataGenerator.RandomPhoneCountryCode(),
                               phoneNumber = DataGenerator.RandomNumber()
                            }
                        }
                    }
                }
            };

            ContactTracingRequest contactTracingRequest = new()
            {
                requests = new List<ContactTracingRequest.Request> { request }
            };

            return JsonConvert.SerializeObject(contactTracingRequest);
        }

        public static string AddOneBagRequest(string passengerNin, string offerItemId, string bagFee)
        {
            JObject addBagsRequest = new()
            {
                ["checkedBagsCount"] = new JArray("1"),
                ["specialItemFlag"] = new JArray("false"),
                ["totalOfferItemid"] = new JArray(offerItemId),
                ["totalMilesFee"] = new JArray("0"),
                ["totalBagFee"] = new JArray(bagFee),
                ["passengerNin"] = new JArray(passengerNin)
            };

            return addBagsRequest.ToString(Formatting.None);
        }

        public static string UpdateBagsRequest(bool pendingPayment)
        {
            JObject updateBagsRequest = new()
            {
                ["pendingPayment"] = pendingPayment
            };

            return updateBagsRequest.ToString(Formatting.None);
        }

        public static string GeneratePaymentReferenceIdRequest(
            string recordLocator,
            string firstName,
            string lastName,
            string birthDate,
            decimal paymentAmount,
            string currencyCode,
            decimal bagFee)
        {
            JObject paymentReferenceIdRequest = new()
            {
                ["conversationId"] = JValue.CreateNull(),
                ["reservationAgentId"] = JValue.CreateNull(),
                ["recordLocatorId"] = JValue.CreateNull(),
                ["paymentAmt"] = new JObject
                {
                    ["currencyEquivalentPrice"] = new JObject
                    {
                        ["countryCode"] = JValue.CreateNull(),
                        ["currencyAmt"] = paymentAmount,
                        ["currencyAmtStr"] = JValue.CreateNull(),
                        ["currencyCode"] = currencyCode,
                        ["decimalPrecisionCnt"] = 0,
                        ["iataDecimalPrecisionCnt"] = JValue.CreateNull(),
                        ["currencySymbol"] = JValue.CreateNull()
                    },
                    ["milesEquivalentPrice"] = JValue.CreateNull()
                },
                ["flightFare"] = new JObject
                {
                    ["countryCode"] = JValue.CreateNull(),
                    ["currencyAmt"] = "0",
                    ["currencyCode"] = currencyCode,
                    ["currencyNbr"] = GetCurrencyNumericCode(currencyCode)
                },
                ["currencyCode"] = currencyCode,
                ["urlGeneratedTimeStampText"] = JValue.CreateNull(),
                ["paymentPortalLifeSpanText"] = JValue.CreateNull(),
                ["statusText"] = JValue.CreateNull(),
                ["offer"] = JValue.CreateNull(),
                ["ecommAdditionalData"] = new JObject
                {
                    ["isMoto"] = JValue.CreateNull(),
                    ["emailAddress"] = DataGenerator.GenerateRandomEmail(),
                    ["ipAddress"] = "127.0.0.1",
                    ["phoneNum"] = "11111111",
                    ["deviceData"] = JValue.CreateNull()
                },
                ["airlineData"] = new JObject
                {
                    ["ticketIssueDate"] = JValue.CreateNull(),
                    ["shopType"] = "Revenue",
                    ["tripType"] = "ONEWAY",
                    ["profile"] = new JObject
                    {
                        ["customer"] = new JObject
                        {
                            ["title"] = JValue.CreateNull(),
                            ["firstName"] = firstName,
                            ["lastName"] = lastName,
                            ["middleName"] = JValue.CreateNull(),
                            ["loyaltyTier"] = JValue.CreateNull(),
                            ["loyaltyId"] = JValue.CreateNull()
                        }
                    },
                    ["passengers"] = new JArray
                    {
                        new JObject
                        {
                            ["passengerSequence"] = 1,
                            ["title"] = JValue.CreateNull(),
                            ["firstName"] = firstName,
                            ["lastName"] = lastName,
                            ["middleName"] = JValue.CreateNull(),
                            ["dateOfBirth"] = birthDate
                        }
                    },
                    ["product"] = new JObject
                    {
                        ["recordLocator"] = recordLocator,
                        ["ticketNumber"] = JValue.CreateNull(),
                        ["productCategory"] = "ANCILLARY",
                        ["productType"] = JValue.CreateNull()
                    },
                    ["bags"] = new JArray
                    {
                        new JObject
                        {
                            ["passengerSequence"] = "1",
                            ["tripTag"] = "1",
                            ["tripSeq"] = "1",
                            ["productDescription"] = "service.CBAG",
                            ["productCurrencyCode"] = currencyCode,
                            ["productCurrencyAmt"] = bagFee,
                            ["productDecimalPrecisionCount"] = "0",
                            ["productCode"] = "CBAG",
                            ["productCategory"] = "SERVICE",
                            ["productItem"] = "2ND CHECKED BAG OR MORE 23KG"
                        }
                    }
                },
                ["additionalConfigParams"] = new JObject
                {
                    ["successRedirectUrl"] = string.Empty,
                    ["failureRedirectUrl"] = string.Empty,
                    ["sessionID"] = JValue.CreateNull(),
                    ["multiProductFlag"] = true,
                    ["insuranceFlag"] = false
                }
            };

            return paymentReferenceIdRequest.ToString(Formatting.None);
        }

        private static JToken GetCurrencyNumericCode(string currencyCode)
        {
            return currencyCode.ToUpperInvariant() switch
            {
                "GBP" => "826",
                "USD" => "840",
                _ => JValue.CreateNull()
            };
        }

        public static string ReviewAndPayDetailsRequest(JObject paymentDetails, string passengerNin)
        {
            JObject reviewAndPayDetailsRequest = new()
            {
                ["creditCardType"] = paymentDetails.RequireToken("paymentMethod.paymentCard.paymentCardTypeCode").ToString(),
                ["creditCardNumber"] = paymentDetails.RequireToken("paymentMethod.paymentCard.paymentCardNum").ToString(),
                ["creditFirstName"] = paymentDetails.RequireToken("paymentMethod.paymentCard.cardHolderName.firstName").ToString(),
                ["creditLastName"] = paymentDetails.RequireToken("paymentMethod.paymentCard.cardHolderName.lastName").ToString(),
                ["expirationMonth"] = paymentDetails.RequireToken("paymentMethod.paymentCard.expirationMonthNum").ToString(),
                ["expirationYear"] = paymentDetails.RequireToken("paymentMethod.paymentCard.expirationYearNum").ToString(),
                ["securityCode"] = paymentDetails.RequireToken("paymentMethod.paymentCard.cardVerificationValueNum").ToString(),
                ["address1"] = paymentDetails.RequireToken("paymentMethod.paymentCard.billingAdr.addressLine1Text").ToString(),
                ["address2"] = paymentDetails.RequireToken("paymentMethod.paymentCard.billingAdr.addressLine2Text").ToString(),
                ["city"] = paymentDetails.RequireToken("paymentMethod.paymentCard.billingAdr.cityLocalityName").ToString(),
                ["country"] = paymentDetails.RequireToken("paymentMethod.paymentCard.billingAdr.countryCode").ToString(),
                ["zipCode"] = paymentDetails.RequireToken("paymentMethod.paymentCard.billingAdr.postalCode").ToString(),
                ["paymentReferenceId"] = paymentDetails.RequireToken("paymentReferenceId").ToString(),
                ["psd2ErrorKey"] = string.Empty,
                ["paymentAmount"] = paymentDetails.RequireToken("price.currencyEquivalentPrice.currencyAmt").ToString(),
                ["paymentAmountDecimal"] = "00",
                ["currencyCode"] = paymentDetails.RequireToken("price.currencyEquivalentPrice.currencyCode").ToString(),
                ["payWithMilesToggle"] = false,
                ["selectedPaxNIN"] = passengerNin
            };

            return reviewAndPayDetailsRequest.ToString(Formatting.None);
        }
    }
}
