using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;
using static VAA.EAPI.Automation.MyAccount.Constants.Application;
using File = System.IO.File;

namespace VAA.EAPI.Automation.MyAccount.Requests.Factory
{
    public static class GraphQL
    {
        public static string BasicAccountEnrolment(string channelUserId, string channelId, string firstName, string lastName, string middleName, string customerType,
            string preferenceType, string preferenceValue, string usage, string emailId, string preferredFlag, string description)
        {
            string payload = File.ReadAllText(@"Requests/Body/GraphQL/Enrolment_Basic_GraphQL.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken requestObjects = payloadObject["variables"]["request"];
            requestObjects["channelUserId"] = channelUserId;
            requestObjects["channelId"] = channelId;

            JToken customerDetails = payloadObject["variables"]["request"]["customerDetails"];
            customerDetails["firstName"] = firstName;
            customerDetails["lastName"] = lastName;
            customerDetails["middleName"] = middleName;
            customerDetails["customerType"] = customerType;

            JToken preferences = payloadObject["variables"]["request"]["customerDetails"]["preferences"][0];
            preferences["preferenceType"] = preferenceType;
            preferences["preferenceValue"] = preferenceValue;

            JToken emailDetails = payloadObject["variables"]["request"]["customerDetails"]["contactDetails"]["emailDetails"];
            emailDetails["usage"] = usage;
            emailDetails["emailId"] = emailId;
            emailDetails["preferredFlag"] = preferredFlag;
            emailDetails["description"] = description;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string InflightEnrolCustomer(string channelId, string enrolmentToken, string firstName, string lastName, string customerType, string enrollmentChannel, string mediaCode, string emailId, string preferenceType, string preferenceValue)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/InflightEnrolCustomer.txt");

            // The Inflight enrolment contract requires the supplied member data and terms acceptance in its variables.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    request = new
                    {
                        channelId,
                        enrolmentToken,
                        customerDetails = new
                        {
                            firstName,
                            lastName,
                            customerType,
                            enrollmentChannel,
                            mediaCode,
                            contactDetails = new { emailDetails = new[] { new { emailId } } },
                            preferences = new[] { new { preferenceType, preferenceValue } },
                            loyaltyMember = new { mediaCode, memberType = "individual", enrollmentChannel = "WEB", termsAndConditionsAccepted = true }
                        }
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string Enrolment(string channelId, string firstName, string lastName, string emailId, string postCode, string countryCode, string password, string dateOfBirth, string enrolmentToken, string enrollmentChannel, string mediaCode, string addressLine1)
        {
            string payload = File.ReadAllText(@"Requests/Body/GraphQL/Enrolment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];

            variables["request"]["channelId"] = channelId;
            variables["request"]["enrolmentToken"] = enrolmentToken;
            variables["request"]["customerDetails"]["firstName"] = firstName;
            variables["request"]["customerDetails"]["lastName"] = lastName;
            variables["request"]["customerDetails"]["contactDetails"]["emailDetails"][0]["emailId"] = emailId;
            variables["request"]["customerDetails"]["contactDetails"]["phoneDetails"][0]["phoneNumber"] = $"{DataGenerator.RandomNumber(11111, 99999, true)}{DataGenerator.RandomNumber(11111, 99999, true)}";
            variables["request"]["customerDetails"]["addressDetails"][0]["addressLine1"] = addressLine1;
            variables["request"]["customerDetails"]["addressDetails"][0]["postalCode"] = postCode;
            variables["request"]["customerDetails"]["addressDetails"][0]["country"] = countryCode;
            variables["request"]["customerDetails"]["password"] = password;
            variables["request"]["customerDetails"]["dateOfBirth"] = dateOfBirth;
            variables["request"]["customerDetails"]["loyaltyMember"]["enrollmentChannel"] = enrollmentChannel;
            variables["request"]["customerDetails"]["loyaltyMember"]["mediaCode"] = mediaCode;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GenerateEmailOtp(string emailId)
        {
            string payload = File.ReadAllText(@"Requests/Body/GraphQL/EmailOtpGeneration.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];

            variables["emailId"] = emailId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string AccountPasswordResetStart(string fcNumberOrEmail)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountPasswordResetStart.txt");

            // Keep the versioned mutation document separate from the caller-supplied reset identifier.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new JObject
                {
                    ["request"] = new JObject
                    { ["fcNumberOrEmail"] = fcNumberOrEmail }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountEntraOtpValidation(string token, string otp, ValidationRequestType requestType)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountEntraOtpValidation.txt");

            // Preserve the service's uppercase validation type while supplying the OTP and enrolment token as request variables.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    request = new
                    {
                        validationRequestType = requestType.ToString().ToUpperInvariant(),
                        oTP = otp,
                        token
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountEntraPasswordSubmission(string token, string password)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountEntraPasswordSubmission.txt");

            // Keep the password out of the mutation document and send it only as a runtime request variable.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    request = new
                    {
                        token,
                        newPassword = password
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string ValidateEmailOtp(string emailId, int otp)
        {
            string payload = File.ReadAllText(@"Requests/Body/GraphQL/ValidateEmailOtp.json");
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];

            variables["emailId"] = emailId;
            variables["otp"] = otp;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string AccountFlightDetails(int page, int pageSize)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountFlightDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.AccountFlightDetails
                {
                    Page = page,
                    PageSize = pageSize
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountUpdateEmailAddress(AccountUpdateEmailAddress request)
        {
            Assert.True(request is not null, "Expected 'request' to be supplied, but found null.");

            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountUpdateEmailAddress.txt");

            // Combine the versioned document with typed variables so the GraphQL contract stays explicit.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = request
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountUpdateAddressDetails(AccountUpdateAddressDetails request)
        {
            Assert.True(request is not null, "Expected 'request' to be supplied, but found null.");

            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountUpdateAddressDetails.txt");

            // Combine the versioned document with typed variables so the GraphQL contract stays explicit.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = request
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string UpdateMemberPhoneNumber(UpdateMemberPhoneNumber request)
        {
            Assert.True(request is not null, "Expected 'request' to be supplied, but found null.");

            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/UpdateMemberPhoneNumber.txt");

            // Combine the versioned document with typed variables so the GraphQL contract stays explicit.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = request
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string RetrieveHouseHoldAccount(string fcNumber, string memberType)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/RetrieveHouseHoldAccount.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.RetrieveHouseHoldAccount
                {
                    RetrieveHhAccountRequest = new Requests.Models.GraphQL.Query.RetrieveHHAccountRequestInput
                    {
                        HouseHoldFcNumber = fcNumber,
                        MemberType = memberType
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountMemberSignIn(string email, string flyingClubNumber, string password)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountMemberSignIn.txt");

            // Keep account credentials in runtime variables rather than in the versioned mutation document.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    request = new
                    {
                        email,
                        flyingClubNumber,
                        password
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountInviteMember(string inviteeFcNumber, string groupName)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountInviteMember.txt");

            // Keep the invited account and requested group as runtime variables for the mutation.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    inviteeFcNumber,
                    groupName
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountInviteMemberAction(string actionType, string inviterFcNumber, string inviteeFcNumber)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountInviteMemberAction.txt");

            // Keep household action details as runtime variables for the mutation.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    request = new
                    {
                        actionType,
                        inviteeFcNumber,
                        inviterFcNumber
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountRefreshToken(string token)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountRefreshToken.txt");

            // Preserve the null scopes field required by the refresh-token request contract.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    request = new
                    {
                        scopes = (string?)null,
                        token
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string ValidatePcmEligibility(string? uniqueId)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/ValidatePcmEligibility.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.ValidatePcmEligibility
                {
                    UniqueId = uniqueId
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AmexRegisterCampaign(string uniqueId, string assignedBy)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AmexRegisterCampaign.txt");

            // Keep campaign-registration identity values in runtime variables rather than in the mutation document.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new
                {
                    registerCampaignRequest = new
                    {
                        createdBy = assignedBy,
                        uniqueId
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string Enrolment(EnrolmentRequest request)
        {
            if (request is null)
                Assert.Fail("Expected an enrolment request to be supplied, but found null.");
            request.Validate();

            string payload = File.ReadAllText(@"Requests/Body/GraphQL/Enrolment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];

            JToken requestObject = variables["request"];
            JToken customerDetails = requestObject["customerDetails"];
            JToken contactDetails = customerDetails["contactDetails"];
            JToken addressDetails = customerDetails["addressDetails"];
            JToken loyaltyMember = customerDetails["loyaltyMember"];
            JToken preferences = customerDetails["preferences"];

            requestObject["channelId"] = request.ChannelId;
            requestObject["enrolmentToken"] = request.EnrolmentToken;

            // Customer Details
            customerDetails["firstName"] = request.Customer.FirstName;
            customerDetails["lastName"] = request.Customer.LastName;
            customerDetails["dateOfBirth"] = request.Customer.DateOfBirth;
            customerDetails["gender"] = request.Customer.Gender;
            customerDetails["password"] = request.Customer.Password;
            customerDetails["customerType"] = request.CustomerType.ToString();

            // Address Details
            addressDetails[0]["addressLine1"] = request.Address.AddressLine1;
            addressDetails[0]["city"] = request.Address.City;
            addressDetails[0]["postalCode"] = request.Address.PostalCode;
            addressDetails[0]["country"] = request.Address.CountryCode;

            // Contact Details
            contactDetails["emailDetails"][0]["emailId"] = request.Contact.EmailId;
            contactDetails["phoneDetails"][0]["phoneNumber"] = request.Contact.PhoneNumber;
            contactDetails["phoneDetails"][0]["countryCode"] = request.Contact.PhoneCountryCode;

            // Loyalty Member Details
            loyaltyMember["enrollmentChannel"] = request.EnrollmentChannel;
            loyaltyMember["mediaCode"] = request.MediaCode;
            loyaltyMember["termsAndConditionsAccepted"] = request.TermsAndConditionsAccepted;

            // Preferences
            preferences.First(p => p["preferenceType"].ToString() == "VaaMarketingPermissions")["preferenceValue"] = request.Preferences.VaaMarketingPermission;
            preferences.First(p => p["preferenceType"].ToString() == "VaPartnersMarketingPermissions")["preferenceValue"] = request.Preferences.VaPartnersMarketingPermission;
            preferences.First(p => p["preferenceType"].ToString() == "VaGroupMarketingPermissions")["preferenceValue"] = request.Preferences.VaGroupMarketingPermissions;
            preferences.First(p => p["preferenceType"].ToString() == "PreferredPartnerFFP")["preferenceValue"] = request.Preferences.PreferredPartnerFFP;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string UpdatePreference(UpdatePreference request)
        {
            if (request is null)
                Assert.Fail("Expected an update-preference request to be supplied, but found null.");

            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/UpdatePreference.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = request

            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountMemberDetails()
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountMemberDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.AccountMemberDetails
                {
                    FlightsCount = 10,
                    ActivitiesCount = 10,
                    VouchersCount = 10
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountManagedYoungFlyerAccounts()
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountManagedYoungFlyerAccounts.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new JObject()
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountPointDotComDetails(string onBehalf)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountPointDotComDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.AccountPointDotComDetails
                {
                    OnBehalf = onBehalf
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountAddressDetails(string addressLine1, string countryCode, string postCode)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountAddressDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.AccountAddressDetails
                {
                    Request = new Requests.Models.GraphQL.Query.AddressDetailRequestInput
                    {
                        AddressLine1 = addressLine1,
                        CountryCode = countryCode,
                        PostCode = postCode
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountActivityDetails(int page, int pageSize)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountActivityDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.AccountActivityDetails
                {
                    Page = page,
                    PageSize = pageSize
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountBannerDetails()
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountBannerDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new JObject()
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string MemberVoucherDetails(int page, int pageSize)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/MemberVoucherDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.MemberVoucherDetails
                {
                    Page = page,
                    PageSize = pageSize
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountManageAccount()
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountManageAccount.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new JObject()
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountTenureDetails()
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountTenureDetails.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new JObject()
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountValidatePassword(string password)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Query/AccountValidatePassword.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Query.AccountValidatePassword
                {
                    Password = new Requests.Models.GraphQL.Query.PasswordValidatorRequestInput
                    {
                        Password = password
                    }
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountEntraOTPGenerate(string emailId)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountEntraOTPGenerate.txt");

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new Requests.Models.GraphQL.Mutation.AccountEntraOTPGenerateRequest
                {
                    EmailId = emailId
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountEntraEnrolMember(AccountEntraEnrolMemberRequest request)
        {
            if (request is null)
                Assert.Fail("Expected an Entra enrolment request to be supplied, but found null.");
            request.Validate();

            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountEntraEnrolMember.txt");

            JObject variables = new()
            {
                ["request"] = new JObject
                {
                    ["channelId"] = request.ChannelId,
                    ["channelUserId"] = request.ChannelUserId,
                    ["enrolmentToken"] = request.EnrolmentToken,
                    ["customerDetails"] = new JObject
                    {
                        ["firstName"] = request.Customer.FirstName,
                        ["lastName"] = request.Customer.LastName,
                        ["dateOfBirth"] = request.Customer.DateOfBirth,
                        ["gender"] = request.Customer.Gender,
                        ["password"] = request.Customer.Password,
                        ["customerType"] = request.CustomerType.ToString(),
                        ["travelProfile"] = new JObject { ["reasonForTravel"] = request.ReasonForTravel },

                        ["loyaltyMember"] = new JObject
                        {
                            ["enrollmentChannel"] = request.LoyaltyMember.EnrollmentChannel,
                            ["mediaCode"] = request.LoyaltyMember.MediaCode,
                            ["termsAndConditionsAccepted"] = request.LoyaltyMember.TermsAndConditionsAccepted,
                            ["status"] = request.LoyaltyMember.Status,
                            ["memberType"] = request.LoyaltyMember.MemberType,
                            ["programName"] = request.LoyaltyMember.ProgramName
                        },

                        ["addressDetails"] = new JArray
                {
                    new JObject
                    {
                        ["addressType"] = request.Address.AddressType,
                        ["country"] = request.Address.CountryCode,
                        ["region"] = request.Address.Region,
                        ["addressLine1"] = request.Address.AddressLine1,
                        ["city"] = request.Address.City,
                        ["postalCode"] = request.Address.PostalCode,
                        ["preferredFlag"] = request.Address.PreferredFlag,
                        ["addressVerified"] = request.Address.AddressVerified
                    }
                },

                        ["contactDetails"] = new JObject
                        {
                            ["emailDetails"] = new JArray
                    {
                        new JObject
                        {
                            ["usage"] = request.Contact.EmailUsage,
                            ["emailId"] = request.Contact.EmailId,
                            ["preferredFlag"] = request.Contact.EmailPreferredFlag
                        }
                    },
                            ["phoneDetails"] = new JArray
                    {
                        new JObject
                        {
                            ["phoneType"] = request.Contact.PhoneType,
                            ["usage"] = request.Contact.PhoneUsage,
                            ["countryCode"] = request.Contact.PhoneCountryCode,
                            ["phoneNumber"] = request.Contact.PhoneNumber,
                            ["preferredFlag"] = request.Contact.PhonePreferredFlag
                        }
                    }
                        },

                        ["preferences"] = new JArray
                {
                    new JObject { ["preferenceType"] = "VaaMarketingPermissions", ["preferenceValue"] = request.Preferences.VaaMarketingPermission },
                    new JObject { ["preferenceType"] = "VaPartnersMarketingPermissions", ["preferenceValue"] = request.Preferences.VaPartnersMarketingPermission },
                    new JObject { ["preferenceType"] = "VaGroupMarketingPermissions", ["preferenceValue"] = request.Preferences.VaGroupMarketingPermissions },
                    new JObject { ["preferenceType"] = "PreferredPartnerFFP", ["preferenceValue"] = request.Preferences.PreferredPartnerFFP }
                }
                    }
                }
            };

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = variables
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string AccountCreateYoungFlyerAccount(string firstName, string lastName, string dateOfBirth, string gender, string channelId)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/AccountCreateYoungFlyerAccount.txt");
            // APP-MYA enrolment persists the App media code; all other supported channels use Web.
            string mediaCode = channelId == "APP-MYA" ? "App" : "Web";

            // The authenticated parent supplies the household context; map only the new Young Flyer details here.
            Requests.Models.GraphQL.Mutation.AccountCreateYoungFlyerAccount request = new()
            {
                Request = new CreateCustomerAccountRequestInput
                {
                    ChannelId = channelId,
                    ChannelUserId = string.Empty,
                    CustomerDetails = new YoungFlyerCustomerDetails
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        DateOfBirth = dateOfBirth,
                        Gender = gender,
                        CustomerType = "Loyalty",
                        LoyaltyMember = new YoungFlyerLoyaltyMember
                        {
                            MemberType = "Individual",
                            MediaCode = mediaCode,
                            EnrollmentChannel = channelId,
                            TermsAndConditionsAccepted = true,
                            Status = "Active",
                            ProgramName = "VirginAtlanticFlyingClub"
                        },
                        Preferences = Array.Empty<object>()
                    }
                }
            };

            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = request
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string ClaimVSPoints(string? ticketNumber, string? fcNumber)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/ClaimVSPoints.txt");

            // Keep the mutation variables in the factory so tests only express the claim scenario.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new JObject
                {
                    ["ticketNumber"] = ticketNumber,
                    ["fcNumber"] = fcNumber
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

        public static string ClaimPartnerFlightPoints(JObject request)
        {
            string query = File.ReadAllText(@"Requests/Body/GraphQL/Mutation/ClaimPartnerFlightPoints.txt");

            // The test supplies the complete Partner-flight request while this factory owns the GraphQL envelope.
            GraphQLRequest graphRequest = new()
            {
                Query = query,
                Variables = new JObject
                {
                    ["request"] = request
                }
            };

            return JsonConvert.SerializeObject(graphRequest);
        }

    }
}
