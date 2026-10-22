using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Delta.Helpers;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;
using VAA.EAPI.Infrastructure.AzureStorage;
using static VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.Helpers
{
    public static class EnrolmentHelper
    {
        public static string env => Config.Environment;
        public static string connectionString = Vault.Default.GetSecretValue($"IdentityStorageAccountConnectionString-{env}");

        private static string DescribeB2cStringToken(JToken? token)
        {
            // Preserve GraphQL response shape so a missing B2C field is not reported as an explicit null value.
            if (token is null || token.Type == JTokenType.Undefined)
            {
                return "<missing>";
            }

            if (token.Type == JTokenType.Null)
            {
                return "<null>";
            }

            return token.Type == JTokenType.String ? $"'{token.Value<string>()}'" : $"<{token.Type}>";
        }

        // Create a per-call storage service so OTP helpers remain parallel-safe (no shared mutable state).
        private static TableStorageService<OtpRequestTableEntity> CreateOtpRequestTableService()
        {
            TableStorageService<OtpRequestTableEntity> otpTableStorageService = new();
            otpTableStorageService.ConnectionString = connectionString;
            otpTableStorageService.TableName = "OTPRequest";
            return otpTableStorageService;
        }

        // Point-read OTP row with retry (avoids table scans + eventual consistency races under parallel runs).
        private static async Task<OtpRequestTableEntity> RetrieveOtpGenerationRecordWithRetryAsync(TableStorageService<OtpRequestTableEntity> otpTableStorageService, string emailId, int expectedOtpLength = 0, int maxAttempts = 30)
        {
            Assert.True(otpTableStorageService is not null, "Expected 'otpTableStorageService' to be supplied, but found null.");
            if (string.IsNullOrWhiteSpace(otpTableStorageService.ConnectionString))
                otpTableStorageService.ConnectionString = connectionString;
            if (string.IsNullOrWhiteSpace(otpTableStorageService.TableName))
                otpTableStorageService.TableName = "OTPRequest";

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                OtpRequestTableEntity record = await otpTableStorageService.RetrieveAsync("OTPGeneration", emailId);
                bool hasExpectedOtp = expectedOtpLength == 0 || record?.OTP.ToString()?.Length == expectedOtpLength;

                if (record != null && hasExpectedOtp)
                    return record;

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            return null;
        }

        public static async Task<int> GetOTPByEmailId(TableStorageService<OtpRequestTableEntity> tableStorageService, string emailId, int expectedOtpLength = 6)
        {
            Assert.True(tableStorageService is not null, "Expected 'tableStorageService' to be supplied, but found null.");
            if (string.IsNullOrEmpty(emailId))
                return 0;

            OtpRequestTableEntity record = await RetrieveOtpGenerationRecordWithRetryAsync(tableStorageService, emailId, expectedOtpLength);
            return record?.OTP is not null ? (int)record.OTP : 0;
        }


        public static async Task<int> GetOTPGenerationCountByEmailId(TableStorageService<OtpRequestTableEntity> tableStorageService, string emailId)
        {
            Assert.True(tableStorageService is not null, "Expected 'tableStorageService' to be supplied, but found null.");
            if (string.IsNullOrEmpty(emailId))
                return 0;

            OtpRequestTableEntity record = await RetrieveOtpGenerationRecordWithRetryAsync(tableStorageService, emailId);
            return record?.OTPGenerationCount is not null ? (int)record.OTPGenerationCount : 0;
        }

        public static async Task<int> GetOTPValidationCountByEmailId(TableStorageService<OtpRequestTableEntity> tableStorageService, string emailId)
        {
            Assert.True(tableStorageService is not null, "Expected 'tableStorageService' to be supplied, but found null.");
            if (string.IsNullOrEmpty(emailId))
                return 0;

            OtpRequestTableEntity record = await RetrieveOtpGenerationRecordWithRetryAsync(tableStorageService, emailId);
            return record?.OTPValidationCount is not null ? (int)record.OTPValidationCount : 0;
        }

        public static async Task<string> GetOTPExpiryTimeByEmailId(TableStorageService<OtpRequestTableEntity> tableStorageService, string emailId)
        {
            Assert.True(tableStorageService is not null, "Expected 'tableStorageService' to be supplied, but found null.");
            if (string.IsNullOrEmpty(emailId))
                return null;

            OtpRequestTableEntity record = await RetrieveOtpGenerationRecordWithRetryAsync(tableStorageService, emailId);
            return record?.OTPExpiryTime;
        }

        public static async Task UpdateOtpExpiryTimeAsync(string email, string expiryTime)
        {
            TableStorageService<OtpRequestTableEntity> tableStorageService = CreateOtpRequestTableService();

            OtpRequestTableEntity record = await tableStorageService.RetrieveAsync("OTPGeneration", email);

            Assert.True(record is not null, "Expected 'record' to be supplied, but found null.");
            record.OTPExpiryTime = expiryTime;

            await tableStorageService.InsertOrMergeAsync(record);
        }

        public static async Task<string> ReturnEnrolmentToken(TableStorageService<OtpRequestTableEntity> tableStorageService, string emailId)
        {
            System.Net.Http.HttpResponseMessage emailOtpResponse = GraphQL.GenerateEmailOtp(env, emailId).EnsureGQLResponseSuccess();

            JObject emailOtpResult = JObject.Parse(await emailOtpResponse.Content.ReadAsStringAsync());
            string message = emailOtpResult.RequireToken("data.accountOTPGenerate").ToString();

            Assert.Equal("OTP generated successfully", message);

            //get OTP from storage
            int otp = await GetOTPByEmailId(tableStorageService, emailId);
            Assert.True(otp != 0, $"OTP should not be zero for the {emailId}");
            Assert.True(otp.ToString().Length.Equals(6), $"OTP is not in 6 digit, OTP is {otp}");

            //get Token
            System.Net.Http.HttpResponseMessage validateOtpResponse = GraphQL.ValidateEmailOtp(env, emailId, otp).EnsureGQLResponseSuccess();
            JObject validateOtpResult = JObject.Parse(await validateOtpResponse.Content.ReadAsStringAsync());
            string enrolmentToken = validateOtpResult.RequireToken("data.accountOTPValidate.enrolmentToken").ToString();
            validateOtpResult.RequireToken("data.accountOTPValidate.isEnrolledMember");

            return enrolmentToken;
        }

        public static async Task<string> ReturnEntraEnrolmentToken(TableStorageService<OtpRequestTableEntity> tableStorageService, string emailId)
        {
            System.Net.Http.HttpResponseMessage emailOtpResponse = GraphQL.AccountEntraOTPGenerate(env, emailId).EnsureGQLResponseSuccess();

            JObject emailOtpResult = JObject.Parse(await emailOtpResponse.Content.ReadAsStringAsync());

            string accountEntraOTPGenerateToken = emailOtpResult.RequireToken("data.accountEntraOTPGenerate").ToString();

            Assert.True(!accountEntraOTPGenerateToken.IsNullOrEmpty(), $"The Email OTP Generate Token should not be null or empty but found empty");

            //get OTP from storage
            int otp = await GetOTPByEmailId(tableStorageService, emailId, 8);
            Assert.True(otp != 0, "OTP should not be zero");
            Assert.True(otp.ToString().Length.Equals(8), $"OTP is not in 8 digit, OTP is {otp}");

            //get Token
            System.Net.Http.HttpResponseMessage validateOtpResponse = GraphQL.AccountEntraOtpValidation(env, accountEntraOTPGenerateToken, otp.ToString(), ValidationRequestType.SignUp).EnsureGQLResponseSuccess();

            string validateOtpBody = await validateOtpResponse.Content.ReadAsStringAsync();
            JObject validateOtpResult = JObject.Parse(validateOtpBody);

            string? continuationToken = validateOtpResult["data"]?["accountEntraOtpValidation"]?["continuationToken"]?.ToString();
            Assert.False(continuationToken.IsNullOrEmpty(), $"OTP Validation Response continuationToken should not be null or empty {validateOtpResult}");

            string enrolmentToken = continuationToken!;
            return enrolmentToken;
        }

        public static async Task ValidateVRedEnrolmentAsync(TokenFixture tokenFixture, string memberNumber)
        {
            Assert.True(tokenFixture is not null, "Expected a token fixture for Azure B2C enrolment validation, but found null.");

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                HttpResponseMessage vRedResponse = Microservice.Vred(tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();
                JObject vRedResult = JObject.Parse(await vRedResponse.Content.ReadAsStringAsync());

                JToken? errorCode = vRedResult.SelectToken("error.code");
                Assert.True(
                    errorCode is null or { Type: JTokenType.Null },
                    $"VRed enrolment failed for member {memberNumber}. Response: {vRedResult}");
                Assert.True(
                    vRedResult.RequireToken("partnerUserId").ToString().Is(memberNumber),
                    $"VRed returned a different partner user ID for member {memberNumber}. Response: {vRedResult}");
            }, 10, 5000);
        }

        public static async Task ValidateAzureB2CEnrolmentAsync(TokenFixture tokenFixture, string memberNumber, bool expectedAccountEnabled = true)
        {
            Assert.True(tokenFixture is not null, "Expected 'tokenFixture' to be supplied, but found null.");

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                HttpResponseMessage azureB2CResponse = Microservice.AzureB2C(tokenFixture.GetToken("azureb2c"), memberNumber).EnsureIsSuccessStatusCode();
                JObject azureB2CResult = JObject.Parse(await azureB2CResponse.Content.ReadAsStringAsync());
                JArray? userCollection = azureB2CResult.RequireToken("value") as JArray;
                Assert.True(userCollection is not null, $"Expected Azure B2C to return a user collection for member '{memberNumber}', but found a non-array value.");
                JArray users = userCollection ?? new JArray();
                JObject? returnedUser = users.FirstOrDefault() as JObject;
                Assert.True(returnedUser is not null, $"Expected Azure B2C to return a user for member '{memberNumber}', but found no user.");
                JObject user = returnedUser ?? new JObject();

                string userPrincipalName = user.RequireToken("userPrincipalName").ToString();
                bool accountEnabled = user.RequireToken("accountEnabled").Value<bool>();

                Assert.True(accountEnabled == expectedAccountEnabled, $"Member '{memberNumber}': expected B2C accountEnabled '{expectedAccountEnabled}', found '{accountEnabled}'.");
            }, 10, 5000);
        }

        public static void ValidateOcdbEnrolment(CertificateFixture certificateFixture, string certificateKey, string memberNumber, string dateOfBirth, string firstName, string lastName, string gender, string email)
        {
            // Import the certificate at the validation boundary so every caller has the required OCDB client certificate.
            certificateFixture.ImportCertificate(certificateKey, Vault.Default, certificateKey);
            System.Xml.XmlDocument? ocdbResponse = null;
            RetryHelper.RunWithRetry(() => ocdbResponse = AIR4Helper.LoyaltyProfileSyncAdd(
                    certificateFixture.GetCertificate(certificateKey),
                    memberNumber,
                    DateTime.Parse(dateOfBirth).ToString("yyyyMMdd"),
                    firstName,
                    lastName,
                    gender,
                    email,
                    out _), maxRetries: 3, delay: 2000);

            Assert.True(ocdbResponse is not null, $"Member '{memberNumber}': expected an OCDB XML response, but no response was returned.");
            System.Xml.XmlDocument completedOcdbResponse = Assert.IsType<System.Xml.XmlDocument>(ocdbResponse);
            System.Xml.XmlNamespaceManager ocdbResponseManager = completedOcdbResponse.CreateXmlNamespaceManager();
            string errorCode = string.Empty;
            string errorText = string.Empty;
            string reasonCode = string.Empty;
            string reasonText = string.Empty;
            string freeformText = string.Empty;
            string messageNumber = string.Empty;
            string sscRequest = string.Empty;

            try
            {
                errorCode = completedOcdbResponse.SelectSingleNode("//error:Error/@ErrorCode", ocdbResponseManager)?.InnerText ?? string.Empty;
                errorText = completedOcdbResponse.SelectSingleNode("//error:Error/@ErrorText", ocdbResponseManager)?.InnerText ?? string.Empty;
                reasonCode = completedOcdbResponse.SelectSingleNode("//error:Error/@ReasonCode", ocdbResponseManager)?.InnerText ?? string.Empty;
                reasonText = completedOcdbResponse.SelectSingleNode("//error:Error/@ReasonText", ocdbResponseManager)?.InnerText ?? string.Empty;
            }
            catch (System.Xml.XPath.XPathException)
            {
                // A successful advisory response does not declare the error namespace prefix.
            }

            try
            {
                freeformText = completedOcdbResponse.SelectSingleNode("//advisorymessage:AdvisoryMessage/@FreeformText", ocdbResponseManager)?.InnerText ?? string.Empty;
                messageNumber = completedOcdbResponse.SelectSingleNode("//advisorymessage:AdvisoryMessage/@MessageNumber", ocdbResponseManager)?.InnerText ?? string.Empty;
                sscRequest = completedOcdbResponse.SelectSingleNode("//advisorymessage:AdvisoryMessage/@SscRequest", ocdbResponseManager)?.InnerText ?? string.Empty;
            }
            catch (System.Xml.XPath.XPathException)
            {
                // An existing-account error response does not declare the advisory namespace prefix.
            }

            if (!freeformText.IsNullOrEmpty() && !messageNumber.IsNullOrEmpty() && !sscRequest.IsNullOrEmpty())
            {
                Assert.True(freeformText.Is("OK"), $"Member '{memberNumber}': expected OCDB FreeformText 'OK', found '{freeformText}'.");
                Assert.True(messageNumber.Is("65535"), $"Member '{memberNumber}': expected OCDB MessageNumber '65535', found '{messageNumber}'.");
                Assert.True(sscRequest.Is("FCFFSC"), $"Member '{memberNumber}': expected OCDB SscRequest 'FCFFSC', found '{sscRequest}'.");
            }
            else if (!errorCode.IsNullOrEmpty() && !errorText.IsNullOrEmpty() && !reasonCode.IsNullOrEmpty() && !reasonText.IsNullOrEmpty())
            {
                Assert.True(errorCode.Is("1501"), $"Member '{memberNumber}': expected OCDB ErrorCode '1501', found '{errorCode}'.");
                Assert.True(errorText.Is("tpf error"), $"Member '{memberNumber}': expected OCDB ErrorText 'tpf error', found '{errorText}'.");
                Assert.True(reasonCode.Is("65535"), $"Member '{memberNumber}': expected OCDB ReasonCode '65535', found '{reasonCode}'.");
                Assert.True(reasonText.Is("FFSC FAILURE - ACCT ALREADY EXISTS"), $"Member '{memberNumber}': expected OCDB ReasonText 'FFSC FAILURE - ACCT ALREADY EXISTS', found '{reasonText}'.");
            }
            else
            {
                Assert.Fail($"Member '{memberNumber}': expected an OCDB success advisory or existing-account error response, found FreeformText '{freeformText}', MessageNumber '{messageNumber}', SscRequest '{sscRequest}', ErrorCode '{errorCode}', ErrorText '{errorText}', ReasonCode '{reasonCode}', and ReasonText '{reasonText}'.");
            }
        }

        public static Dictionary<string, string> GetFailingPasswords()
        {
            return new Dictionary<string, string>
                {
                    // PP01: Length must be between 8 and 20 characters
                    { "PP01_LengthTooShort", "Ab1!" },                     // Too short
                    { "PP01_LengthTooLong", "ThisPasswordIsWayTooLong123!" }, // Too long

                    // PP02: Must contain at least 1 number
                    { "PP02_NoNumber", "Password!" },

                    // PP03: Must contain at least 1 uppercase letter
                    { "PP03_NoUppercase", "password1!" },

                    // PP04: Must contain at least 1 lowercase letter
                    { "PP04_NoLowercase", "PASSWORD1!" },

                    // PP05: Must contain no more than 3 special characters
                    { "PP05_TooManySpecials", "Pass123!@#$" },

                    // PP06: Must not contain the '@' symbol
                    { "PP06_ContainsAtSymbol", "Pass123@!" }
                };
        }

        public record AddressWithPhoneCode(string Name, string AddressLine1, string AddressLine2, string City, string PostalCode, string CountryCode, string PhoneCountryCode);

        public record PreferenceSet(string VaaMarketingPermissions, string VaPartnersMarketingPermissions, string VaGroupMarketingPermissions, string PreferredPartnerFFP);

        public static readonly AddressWithPhoneCode[] AddressData =
        {
              // UK (GB / 44)
             new("UK_London", "10 Downing Street", "", "London", "SW1A 2AA", "GB", "44"),
             new("UK_Southampton", "1 High Street", "", "Southampton", "SO14 0LT", "GB", "44"),
             new("UK_Manchester", "1 St Peter's Square", "", "Manchester", "M2 3AE", "GB", "44"),
             new("UK_Birmingham", "Centenary Square", "", "Birmingham", "B1 2EA", "GB", "44"),
             new("UK_Edinburgh", "50 Princes Street", "", "Edinburgh", "EH2 2BY", "GB", "44"),
             new("UK_Glasgow", "1 George Square", "", "Glasgow", "G2 1DU", "GB", "44"),
             new("UK_Bristol", "1 College Green", "", "Bristol", "BS1 5TJ", "GB", "44"),

             // US (US / 1)
             new("US_NewYork", "11 Wall Street", "", "New York", "10005", "US", "1"),
             new("US_SanFrancisco", "1355 Market St", "Suite 900", "San Francisco", "94103", "US", "1"),
             new("US_Chicago", "233 S Wacker Dr", "", "Chicago", "60606", "US", "1"),
             new("US_Seattle", "701 5th Ave", "", "Seattle", "98104", "US", "1"),
             new("US_Miami", "200 S Biscayne Blvd", "", "Miami", "33131", "US", "1"),

             // Non-UK/US (few) — no region
             new("IE_Dublin", "1 Grand Canal Square", "", "Dublin", "D02 H210", "IE", "353"),
             new("CA_Toronto", "100 Queen St W", "", "Toronto", "M5H 2N2", "CA", "1"),
             new("AU_Sydney", "48 Pirrama Rd", "", "Pyrmont", "2009", "AU", "61"),
             new("DE_Berlin", "Pariser Platz", "", "Berlin", "10117", "DE", "49"),
             new("IN_NewDelhi", "11 Janpath Road", "", "New Delhi", "110001", "IN", "91"),
             new("SG_Singapore", "10 Anson Rd", "", "Singapore", "079903", "SG", "65"),
     };

        public enum Country
        {
            GB = 0,
            US = 1
        }

        public static readonly PreferenceSet[] PreferenceSets =
     {
             new( "N", "N", "N","AF"),
             new( "Y", "Y", "Y","DL"),
             new( "N", "N", "Y","KL"),
             new( "N", "Y", "N","NZ"),
             new( "Y", "N", "N","SQ")

    };

        public static AddressWithPhoneCode PickRandomAddress(Country countryCode) =>
                AddressData.Where(a => a.CountryCode == countryCode.ToString()).OrderBy(_ => Random.Shared.Next()).First();

        public static AddressWithPhoneCode PickRandomNonUkUsAddress() =>
            AddressData.Where(a => a.CountryCode is not "GB" and not "US").OrderBy(_ => Random.Shared.Next()).First();

        public static PreferenceSet PickRandomPreferences() => PreferenceSets[Random.Shared.Next(PreferenceSets.Length)];

        public static async Task<EntraLoyaltyAccount> CreateEntraLoyaltyAccountAsync(string enrollmentChannel, string mediaCode, string firstName, string lastName, string emailId, string dateOfBirth, string loyaltyEnrollmentChannel = "", bool forHousehold = false)
        {
            Country[] countries = Enum.GetValues<Country>();
            AddressWithPhoneCode addressDetails = forHousehold
                ? new AddressWithPhoneCode("Household", "Test Address Line 1", "Test Address Line 2", "London", "RH117DU", "GB", "44")
                : PickRandomAddress(countries[Random.Shared.Next(countries.Length)]);
            PreferenceSet preferences = PickRandomPreferences();
            string phoneNumber = Random.Shared.NextInt64(7000000000, 9999999999).ToString("D10").TrimStart('0');

            // Entra OTP requests use the shared SIT table because SIT and UAT use the same identity tenant.
            TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new()
            {
                ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"),
                TableName = "OTPRequest"
            };
            string enrolmentToken = await ReturnEntraEnrolmentToken(otpRequestTableStorageService, emailId);
            string effectiveLoyaltyEnrollmentChannel = loyaltyEnrollmentChannel.IsNullOrWhiteSpace() ? enrollmentChannel : loyaltyEnrollmentChannel;

            AccountEntraEnrolMemberRequest enrolmentRequest = new()
            {
                EnrolmentToken = enrolmentToken,
                ChannelId = enrollmentChannel,
                CustomerType = CustomerType.Loyalty,
                Customer = new() { FirstName = firstName, LastName = lastName, DateOfBirth = dateOfBirth, Gender = MyAccount.Constants.Application.GenderTypes[Random.Shared.Next(MyAccount.Constants.Application.GenderTypes.Length)], Password = MyAccount.Constants.Application.LoginPassword },
                Address = new() { AddressLine1 = addressDetails.AddressLine1, City = addressDetails.City, PostalCode = addressDetails.PostalCode, CountryCode = addressDetails.CountryCode },
                Contact = new() { EmailId = emailId, PhoneCountryCode = addressDetails.PhoneCountryCode, PhoneNumber = phoneNumber },
                LoyaltyMember = new() { EnrollmentChannel = effectiveLoyaltyEnrollmentChannel, MediaCode = mediaCode },
                Preferences = new() { VaaMarketingPermission = preferences.VaaMarketingPermissions, VaPartnersMarketingPermission = preferences.VaPartnersMarketingPermissions, VaGroupMarketingPermissions = preferences.VaGroupMarketingPermissions, PreferredPartnerFFP = preferences.PreferredPartnerFFP }
            };

            // Avoid shared GraphQL failure diagnostics because this request contains the configured test password.
            using HttpResponseMessage enrolmentResponse = GraphQL.AccountEntraEnrolMember(env, enrolmentRequest);
            JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
            bool enrolmentHasErrors = enrolmentResult["errors"] is JArray { Count: > 0 };

            Assert.True(enrolmentResponse.IsSuccessStatusCode, $"Expected accountEntraEnrolMember to return HTTP 200, but found HTTP {(int)enrolmentResponse.StatusCode}.");
            Assert.False(enrolmentHasErrors, "Expected accountEntraEnrolMember to complete without GraphQL errors, but found an error response.");
            string memberNumber = enrolmentResult.RequireToken("data.accountEntraEnrolMember.memberNumber").ToString();
            string idToken = enrolmentResult.RequireToken("data.accountEntraEnrolMember.idToken").ToString();
            using HttpResponseMessage memberDetailsResponse = GraphQL.AccountMemberDetails(env, idToken).EnsureGQLResponseSuccess();
            JObject memberDetailsResult = JObject.Parse(await memberDetailsResponse.Content.ReadAsStringAsync());
            string customerId = memberDetailsResult.RequireToken("data.accountMemberDetails.flyingClubMemberDetails.customerID").ToString();

            Assert.False(memberNumber.IsNullOrWhiteSpace(), "Expected a non-empty member number from Entra enrolment, found '<empty>'.");
            Assert.False(customerId.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a non-empty customer identifier, but no identifier was returned.");

            return new EntraLoyaltyAccount(memberNumber, customerId, idToken);
        }

        public static async Task<string> CreateIndividualUnderEighteenFacadeLoyaltyAccountAsync(string enrollmentChannel, string firstName, string lastName, string emailId, string dateOfBirth)
        {
            DateTime parsedDateOfBirth = DateTime.Parse(dateOfBirth);
            Assert.True(parsedDateOfBirth > DateTime.UtcNow.Date.AddYears(-18), $"Expected an under-18 date of birth for Facade enrolment, but found '{dateOfBirth}'.");

            AddressWithPhoneCode addressDetails = PickRandomAddress(Country.GB);
            TokenFixture tokenFixture = new();
            tokenFixture.AddTokenForDynamicsEnrolmentFromKeyVault("enrolment");

            // The Enrolment Facade remains the supported path for creating individual under-18 accounts.
            using HttpResponseMessage enrolmentResponse = Microservice.Enrolment(
                env,
                tokenFixture.GetToken("enrolment"),
                enrollmentChannel,
                firstName,
                lastName,
                emailId,
                addressDetails.PostalCode,
                addressDetails.CountryCode,
                output: null,
                dateOfBirth: dateOfBirth,
                city: addressDetails.City,
                addressLine1: addressDetails.AddressLine1,
                memberType: "Individual",
                guardianTermsAndConditionsAccepted: true,
                phoneCountryCode: addressDetails.PhoneCountryCode,
                region: string.Empty);

            Assert.True(
                enrolmentResponse.StatusCode == System.Net.HttpStatusCode.Accepted,
                $"Expected Enrolment Facade to return '{System.Net.HttpStatusCode.Accepted}', but found '{enrolmentResponse.StatusCode}'.");

            JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
            string customerId = enrolmentResult.RequireToken("customerId").ToString();
            string memberNumber = enrolmentResult.RequireToken("memberdetails.memberNumber").ToString();

            Assert.False(customerId.IsNullOrWhiteSpace(), "Expected Facade enrolment to return a customer ID, but found an empty value.");
            Assert.False(memberNumber.IsNullOrWhiteSpace(), "Expected Facade enrolment to return a member number, but found an empty value.");

            // Facade provisioning is asynchronous, so retry the password update briefly before Book authenticates the member.
            RetryHelper.RunWithRetry(() =>
            {
                using HttpResponseMessage passwordResetResponse = IdentityFacadeApi.ResetPassword(memberNumber, MyAccount.Constants.Application.LoginPassword);
                Assert.True(passwordResetResponse.IsSuccessStatusCode, $"Expected Identity Facade password update to return a successful HTTP status, but found '{passwordResetResponse.StatusCode}'.");
            }, maxRetries: 5, delay: 1000);

            return memberNumber;
        }

        public async static Task<(string, string)> GraphQLEnrolment(bool underSixteen = false)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string dateOfBirth = underSixteen ? DateTime.UtcNow.AddYears(-14).ToString("yyyy-MM-dd") : DateTime.UtcNow.AddYears(-30).AddMonths(Random.Shared.Next(1, 12)).ToString("yyyy-MM-dd");
            string email = firstName.ToLowerInvariant() + lastName.ToLowerInvariant() + Random.Shared.NextInt64(100000000000, 999999999999) +
            "@example.com";
            string enrollmentChannel = "WEB-MYA";
            string mediaCode = "App";

            if (underSixteen)
            {
                // Retain the legacy mutation temporarily because Entra now rejects direct under-18 enrolment.
                AddressWithPhoneCode addressDetails = PickRandomAddress((Country)Random.Shared.Next(0, 2));
                PreferenceSet preferences = PickRandomPreferences();
                TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = CreateOtpRequestTableService();
                string enrolmentToken = await ReturnEnrolmentToken(otpRequestTableStorageService, email);
                EnrolmentRequest enrolmentRequest = new()
                {
                    EnrolmentToken = enrolmentToken,
                    EnrollmentChannel = enrollmentChannel,
                    MediaCode = mediaCode,
                    ChannelId = enrollmentChannel,
                    CustomerType = CustomerType.Loyalty,
                    Customer = new() { FirstName = firstName, LastName = lastName, DateOfBirth = dateOfBirth, Gender = MyAccount.Constants.Application.GenderTypes[Random.Shared.Next(MyAccount.Constants.Application.GenderTypes.Length)], Password = MyAccount.Constants.Application.LoginPassword },
                    Address = new() { AddressLine1 = addressDetails.AddressLine1, City = addressDetails.City, PostalCode = addressDetails.PostalCode, CountryCode = addressDetails.CountryCode },
                    Contact = new() { EmailId = email, PhoneCountryCode = addressDetails.PhoneCountryCode, PhoneNumber = Random.Shared.NextInt64(7000000000, 9999999999).ToString("D10").TrimStart('0') },
                    Preferences = new() { VaaMarketingPermission = preferences.VaaMarketingPermissions, VaPartnersMarketingPermission = preferences.VaPartnersMarketingPermissions, VaGroupMarketingPermissions = preferences.VaGroupMarketingPermissions, PreferredPartnerFFP = preferences.PreferredPartnerFFP }
                };

                using HttpResponseMessage enrolmentResponse = GraphQL.Enrolment(env, enrolmentRequest).EnsureGQLResponseSuccess();
                JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
                string customerId = enrolmentResult.RequireToken("data.accountEnrolMember.customerId").ToString();
                string memberNumber = enrolmentResult.RequireToken("data.accountEnrolMember.memberDetails.memberNumber").ToString();

                await ValidateAccountInVRedAndB2CAsync(memberNumber, email, firstName, lastName);
                return (customerId, memberNumber);
            }

            EntraLoyaltyAccount account = await CreateEntraLoyaltyAccountAsync(enrollmentChannel, mediaCode, firstName, lastName, email, dateOfBirth);
            await ValidateAccountInVRedAndB2CAsync(account.MemberNumber, email, firstName, lastName, useCardLinksForVRedValidation: true);

            return (account.CustomerId, account.MemberNumber);
        }

        public static async Task<(string emailId, string loyaltyAccountNumber, string customerId, string firstName, string lastName)> GraphQLEnrolment(string enrollmentChannel, string mediaCode, bool forHousehold = false)
        {
            // Preserve the established five-value contract while delegating enrolment to the canonical Entra flow.
            (string emailId, string loyaltyAccountNumber, string customerId, string firstName, string lastName, _, _, _, _) = await EntraEnrolment(enrollmentChannel, mediaCode, forHousehold);

            return (emailId, loyaltyAccountNumber, customerId, firstName, lastName);
        }

        public static async Task<(string emailId, string loyaltyAccountNumber, string customerId, string firstName, string lastName, string idToken, DateTime dateOfBirth, string phoneNumber, string countryCode)> EntraEnrolment(string enrollmentChannel, string mediaCode, bool forHousehold = false)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            DateTime dateOfBirth = DateTime.UtcNow.Date.AddYears(-30).AddMonths(Random.Shared.Next(1, 12));
            string dateOfBirthValue = dateOfBirth.ToString("yyyy-MM-dd");
            string email = firstName.ToLowerInvariant() + lastName.ToLowerInvariant() + Random.Shared.NextInt64(100000000000, 999999999999) + "@example.com";
            string gender = MyAccount.Constants.Application.GenderTypes[Random.Shared.Next(0, MyAccount.Constants.Application.GenderTypes.Length)];

            Country[] countries = Enum.GetValues<Country>();
            Country country = countries[Random.Shared.Next(countries.Length)];
            AddressWithPhoneCode addressDetails = forHousehold
                ? new AddressWithPhoneCode(
                    "Household",
                    "Test Address Line 1",
                    "Test Address Line 2",
                    "London",
                    "RH117DU",
                    "GB",
                    "44")
                : PickRandomAddress(country);
            PreferenceSet preferences = PickRandomPreferences();
            string phoneNumber = Random.Shared.NextInt64(7000000000, 9999999999).ToString("D10").TrimStart('0');

            // Entra OTP requests use the shared SIT table because SIT and UAT use the same identity tenant.
            TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new()
            {
                ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"),
                TableName = "OTPRequest"
            };
            string enrolmentToken = await ReturnEntraEnrolmentToken(otpRequestTableStorageService, email);
            AccountEntraEnrolMemberRequest enrolmentRequest = new()
            {
                EnrolmentToken = enrolmentToken,
                ChannelId = enrollmentChannel,
                CustomerType = CustomerType.Loyalty,
                Customer = new()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    DateOfBirth = dateOfBirthValue,
                    Gender = gender,
                    Password = MyAccount.Constants.Application.LoginPassword
                },
                Address = new()
                {
                    AddressLine1 = addressDetails.AddressLine1,
                    City = addressDetails.City,
                    PostalCode = addressDetails.PostalCode,
                    CountryCode = addressDetails.CountryCode
                },
                Contact = new()
                {
                    EmailId = email,
                    PhoneCountryCode = addressDetails.PhoneCountryCode,
                    PhoneNumber = phoneNumber
                },
                LoyaltyMember = new()
                {
                    EnrollmentChannel = enrollmentChannel,
                    MediaCode = mediaCode
                },
                Preferences = new()
                {
                    VaaMarketingPermission = preferences.VaaMarketingPermissions,
                    VaPartnersMarketingPermission = preferences.VaPartnersMarketingPermissions,
                    VaGroupMarketingPermissions = preferences.VaGroupMarketingPermissions,
                    PreferredPartnerFFP = preferences.PreferredPartnerFFP
                }
            };

            // The Entra response supplies the bearer token required by parent-authorized account operations.
            using HttpResponseMessage enrolmentResponse = GraphQL.AccountEntraEnrolMember(env, enrolmentRequest).EnsureGQLResponseSuccess();
            JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
            string idToken = enrolmentResult.RequireToken("data.accountEntraEnrolMember.idToken").ToString();
            string memberNumber = enrolmentResult.RequireToken("data.accountEntraEnrolMember.memberNumber").ToString();
            string returnedFirstName = enrolmentResult.RequireToken("data.accountEntraEnrolMember.firstName").ToString();
            string returnedLastName = enrolmentResult.RequireToken("data.accountEntraEnrolMember.lastName").ToString();

            Assert.False(idToken.IsNullOrWhiteSpace(), $"Parent member '{memberNumber}': expected a non-empty Entra ID token, but no token was returned.");
            Assert.False(memberNumber.IsNullOrWhiteSpace(), "Expected a non-empty member number from Entra enrolment, found '<empty>'.");
            Assert.True(returnedFirstName.Is(firstName), $"Parent member '{memberNumber}': expected first name '{firstName}', found '{returnedFirstName}'.");
            Assert.True(returnedLastName.Is(lastName), $"Parent member '{memberNumber}': expected last name '{lastName}', found '{returnedLastName}'.");

            using HttpResponseMessage memberDetailsResponse = GraphQL.AccountMemberDetails(env, idToken).EnsureGQLResponseSuccess();
            JObject memberDetailsResult = JObject.Parse(await memberDetailsResponse.Content.ReadAsStringAsync());
            string customerId = memberDetailsResult.RequireToken("data.accountMemberDetails.flyingClubMemberDetails.customerID").ToString();

            Assert.False(customerId.IsNullOrWhiteSpace(), $"Parent member '{memberNumber}': expected a non-empty customer identifier, but no identifier was returned.");

            await ValidateAccountInVRedAndB2CAsync(memberNumber, email, firstName, lastName, useCardLinksForVRedValidation: true);

            return (email, memberNumber, customerId, firstName, lastName, idToken, dateOfBirth, phoneNumber, addressDetails.PhoneCountryCode);
        }

        private static async Task ValidateAccountInVRedAndB2CAsync(string memberNumber, string email, string firstName, string lastName, bool useCardLinksForVRedValidation = false)
        {
            TokenFixture tokenFixture = new();
            tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");
            tokenFixture.AddTokenForAzureB2CFromKeyVault("azureGraphB2C");

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                if (useCardLinksForVRedValidation)
                {
                    // CardLinks confirms the enrolled member's identity, active status, zero balance, and Red user link.
                    using HttpResponseMessage vRedCardLinksResponse = Microservice.VRedMSCardLinks(env, tokenFixture.GetToken("VRedMS"), memberNumber).EnsureIsSuccessStatusCode();
                    JObject vRedCardLinksResult = JObject.Parse(await vRedCardLinksResponse.Content.ReadAsStringAsync());
                    string partnerUserId = vRedCardLinksResult.RequireToken("partnerUserId").ToString();
                    string accountStatus = vRedCardLinksResult.RequireToken("accountStatus").ToString();
                    string cardLinksFirstName = vRedCardLinksResult.RequireToken("firstName").ToString();
                    string cardLinksLastName = vRedCardLinksResult.RequireToken("lastName").ToString();
                    string cardLinksEmail = vRedCardLinksResult.RequireToken("email").ToString();
                    string redUserId = vRedCardLinksResult.RequireToken("redUserId").ToString();
                    string currentBalance = vRedCardLinksResult.RequireToken("balance.current").ToString();
                    string usableBalance = vRedCardLinksResult.RequireToken("balance.usable").ToString();
                    JArray cardLinks = Assert.IsType<JArray>(vRedCardLinksResult.RequireToken("cardLinks"));

                    Assert.True(partnerUserId.Is(memberNumber), $"Member '{memberNumber}': expected CardLinks partnerUserId '{memberNumber}', found '{partnerUserId}'.");
                    Assert.True(accountStatus.Is("ACTIVE"), $"Member '{memberNumber}': expected CardLinks accountStatus 'ACTIVE', found '{accountStatus}'.");
                    Assert.True(cardLinksFirstName.Is(firstName), $"Member '{memberNumber}': expected CardLinks firstName '{firstName}', found '{cardLinksFirstName}'.");
                    Assert.True(cardLinksLastName.Is(lastName), $"Member '{memberNumber}': expected CardLinks lastName '{lastName}', found '{cardLinksLastName}'.");
                    Assert.True(cardLinksEmail.Is(email), $"Member '{memberNumber}': expected CardLinks email '{email}', found '{cardLinksEmail}'.");
                    Assert.False(redUserId.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated CardLinks redUserId, but found '{redUserId}'.");
                    Assert.True(currentBalance.Is("0"), $"Member '{memberNumber}': expected CardLinks current balance '0', found '{currentBalance}'.");
                    Assert.True(usableBalance.Is("0"), $"Member '{memberNumber}': expected CardLinks usable balance '0', found '{usableBalance}'.");
                    Assert.Empty(cardLinks);
                }
                else
                {
                    using HttpResponseMessage vRedResponse = Microservice.VredPointsMsEndPoint(env, tokenFixture.GetToken("VRedMS"), memberNumber).EnsureIsSuccessStatusCode();
                    JObject vRedResult = JObject.Parse(await vRedResponse.Content.ReadAsStringAsync());
                    JToken? current = vRedResult.SelectToken("Current");
                    string currentValue = current?.ToString() ?? "<missing>";

                    Assert.True(
                        current is { Type: not JTokenType.Null and not JTokenType.Undefined },
                        $"Member '{memberNumber}': expected a non-empty VRed 'Current' value, found '{currentValue}'.");
                }
            }, 5, 4000);

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                HttpResponseMessage b2cResponse = MicrosoftGraph.GetUserDetailsByEmailOrFCNumber(tokenFixture.GetToken("azureGraphB2C"), email).EnsureIsSuccessStatusCode();

                JObject b2cResult = JObject.Parse(await b2cResponse.Content.ReadAsStringAsync());

                JToken b2cUsersToken = b2cResult.RequireToken("value");
                Assert.True(b2cUsersToken is JArray, $"Member '{memberNumber}': expected B2C 'value' to be an array, found JSON type '{b2cUsersToken.Type}'.");
                JArray b2cUsers = Assert.IsType<JArray>(b2cUsersToken);
                Assert.True(b2cUsers.Count == 1, $"Member '{memberNumber}': expected exactly one B2C user, found '{b2cUsers.Count}'.");
                JObject b2cUser = Assert.IsType<JObject>(b2cUsers[0]);
                // Preserve missing B2C names so the assertion reports the downstream contract failure instead of masking it as a JSON-path exception.
                JToken? b2cGivenNameToken = b2cUser.SelectToken("givenName");
                JToken? b2cSurnameToken = b2cUser.SelectToken("surname");
                bool b2cGivenNameMatches = b2cGivenNameToken is { Type: JTokenType.String } && b2cGivenNameToken.Value<string>().Is(firstName);
                bool b2cSurnameMatches = b2cSurnameToken is { Type: JTokenType.String } && b2cSurnameToken.Value<string>().Is(lastName);
                bool b2cAccountEnabled = b2cUser.RequireToken("accountEnabled").Value<bool>();
                bool hasEmailIdentity = b2cUser.SelectTokens("identities[*]")
                    .Where(identity => identity.RequireToken("signInType").ToString().Is("emailAddress"))
                    .Select(identity => identity.RequireToken("issuerAssignedId").ToString())
                    .Any(issuerAssignedId => issuerAssignedId.Is(email));
                bool hasMemberNumberIdentity = b2cUser.SelectTokens("identities[*]")
                    .Where(identity => identity.RequireToken("signInType").ToString().Is("username"))
                    .Select(identity => identity.RequireToken("issuerAssignedId").ToString())
                    .Any(issuerAssignedId => issuerAssignedId.Is(memberNumber));

                Assert.True(b2cGivenNameMatches, $"Member '{memberNumber}': expected B2C given name '{firstName}', but found {DescribeB2cStringToken(b2cGivenNameToken)}.");
                Assert.True(b2cSurnameMatches, $"Member '{memberNumber}': expected B2C surname '{lastName}', but found {DescribeB2cStringToken(b2cSurnameToken)}.");
                Assert.True(b2cAccountEnabled, $"Member '{memberNumber}': expected B2C accountEnabled 'True', found '{b2cAccountEnabled}'.");
                Assert.True(hasEmailIdentity, $"Member '{memberNumber}': expected an email-address sign-in identity matching the enrolment request, but none was found.");
                Assert.True(hasMemberNumberIdentity, $"Member '{memberNumber}': expected a username sign-in identity matching member number '{memberNumber}', but none was found.");
            }, 5, 2000);
        }


    }
}
