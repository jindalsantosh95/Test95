using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;
using static VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.Requests.Endpoint
{
    public static class GraphQL
    {
        public static HttpResponseMessage BasicAccountEnrolment(string env, string channelUserId, string channelId, string firstName, string lastName, string middleName, string customerType, string preferenceType, string preferenceValue, string usage, string emailId, string preferredFlag, string description)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlLoyalty")}"),
                Factory.GraphQL.BasicAccountEnrolment(channelUserId, channelId, firstName, lastName, middleName, customerType,
            preferenceType, preferenceValue, usage, emailId, preferredFlag, description));
        }

        /// <summary>Enrols a customer in Flying Club during an in-flight journey.</summary>
        public static HttpResponseMessage InflightEnrolCustomer(string env, string channelId, string enrolmentToken, string firstName, string lastName, string customerType, string enrollmentChannel, string mediaCode, string emailId, string preferenceType, string preferenceValue)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"),
                Factory.GraphQL.InflightEnrolCustomer(channelId, enrolmentToken, firstName, lastName, customerType, enrollmentChannel, mediaCode, emailId, preferenceType, preferenceValue));
        }

        public static HttpResponseMessage SetDeviceInflightState(string env)
        {
            string payloadPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Requests", "Body", "GraphQL", "SetDeviceInflightState.json");
            string payload = File.ReadAllText(payloadPath);

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), payload);
        }

        public static HttpResponseMessage AccountEnableWifiForDevice(string env, string token)
        {
            string payloadPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Requests", "Body", "GraphQL", "AccountEnableWifiForDevice.json");
            string payload = File.ReadAllText(payloadPath);

            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), payload, headers);
        }

        public static HttpResponseMessage Enrolment(string env, string channelID, string firstName, string lastName, string emailId, string postCode, string countryCode, string enrolmentToken, string enrollmentChannel, string mediaCode, string password = "Virgin123", string dateOfBirth = "", string addressLine1 = "")
        {
            if (dateOfBirth.IsNullOrWhiteSpace())
                dateOfBirth = "1990-01-12";

            if (addressLine1.IsNullOrWhiteSpace())
                addressLine1 = "Tramway1";

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"),
                Factory.GraphQL.Enrolment(channelID, firstName, lastName, emailId, postCode, countryCode, password, dateOfBirth, enrolmentToken, enrollmentChannel, mediaCode, addressLine1));
        }

        public static HttpResponseMessage Enrolment(string env, EnrolmentRequest request)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"),
                Factory.GraphQL.Enrolment(request));
        }

        public static HttpResponseMessage GenerateEmailOtp(string env, string emailId)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.GenerateEmailOtp(emailId));
        }

        public static HttpResponseMessage ValidateEmailOtp(string env, string emailId, int otp)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.ValidateEmailOtp(emailId, otp));
        }

        /// <summary>Retrieves the member profile, loyalty balance, vouchers, activities, and upcoming flights.</summary>
        public static HttpResponseMessage AccountMemberDetails(string env, string token)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountMemberDetails(), headers);
        }

        /// <summary>Retrieves Young Flyer accounts managed by the authenticated parent member.</summary>
        public static HttpResponseMessage AccountManagedYoungFlyerAccounts(string env, string token)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", $"Bearer {token}" }
            };

            // The bearer token scopes this no-variable query to the authenticated parent account.
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountManagedYoungFlyerAccounts(), headers);
        }

        /// <summary>Retrieves the Points.com details for the nominated member.</summary>
        public static HttpResponseMessage AccountPointDotComDetails(string env, string token, string onBehalf)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountPointDotComDetails(onBehalf), headers);
        }

        /// <summary>Retrieves a paged history of the member's loyalty activity and points transactions.</summary>
        public static HttpResponseMessage AccountActivityDetails(string env, string token, int page = 1, int pageSize = 2)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountActivityDetails(page, pageSize), headers);
        }

        /// <summary>Retrieves the member's account banner display state.</summary>
        public static HttpResponseMessage AccountBannerDetails(string env, string token)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", $"Bearer {token}" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountBannerDetails(), headers);
        }

        /// <summary>Retrieves a paged list of the member's vouchers and voucher categories.</summary>
        public static HttpResponseMessage MemberVoucherDetails(string env, string token, int page = 1, int pageSize = 2)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.MemberVoucherDetails(page, pageSize), headers);
        }

        /// <summary>Retrieves the member profile, contact details, address details, and household leadership details.</summary>
        public static HttpResponseMessage AccountManageAccount(string env, string token)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountManageAccount(), headers);
        }

        /// <summary>Retrieves the member's active-year tenure history and associated benefits.</summary>
        public static HttpResponseMessage AccountTenureDetails(string env, string token)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountTenureDetails(), headers);
        }

        /// <summary>Validates a password against the account password policy.</summary>
        public static HttpResponseMessage AccountValidatePassword(string env, string password)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountValidatePassword(password));
        }

        /// <summary>Validates and retrieves standardised details for a supplied address.</summary>
        public static HttpResponseMessage AccountAddressDetails(string env, string addressLine1, string countryCode, string postCode)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountAddressDetails(addressLine1, countryCode, postCode));
        }

        /// <summary>Retrieves a paged list of the member's flight segments.</summary>
        public static HttpResponseMessage AccountFlightDetails(string env, string token, int page = 1, int pageSize = 10)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", $"Bearer {token}" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountFlightDetails(page, pageSize), headers);
        }

        /// <summary>Updates the authenticated member's email address.</summary>
        public static HttpResponseMessage AccountUpdateEmailAddress(string env, string token, AccountUpdateEmailAddress request)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            // Send member changes through the typed request factory so documents and variables stay aligned.
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountUpdateEmailAddress(request), headers);
        }

        /// <summary>Updates the authenticated member's address details.</summary>
        public static HttpResponseMessage AccountUpdateAddressDetails(string env, string token, AccountUpdateAddressDetails request)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            // Send member changes through the typed request factory so documents and variables stay aligned.
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountUpdateAddressDetails(request), headers);
        }

        /// <summary>Updates the authenticated member's phone number.</summary>
        public static HttpResponseMessage UpdateMemberPhoneNumber(string env, string token, UpdateMemberPhoneNumber request)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            // Send member changes through the typed request factory so documents and variables stay aligned.
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.UpdateMemberPhoneNumber(request), headers);
        }

        /// <summary>Retrieves the household leader, linked members, and outstanding invitations.</summary>
        public static HttpResponseMessage RetrieveHouseHoldAccount(string env, string token, string fcNumber, string memberType)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.RetrieveHouseHoldAccount(fcNumber, memberType), headers);
        }
        /// <summary>Authenticates a member and returns the account sign-in tokens.</summary>
        public static HttpResponseMessage AccountMemberSignIn(string env, string email, string loyaltyAccountNumber, string password)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountMemberSignIn(email, loyaltyAccountNumber, password));
        }
        /// <summary>Starts a password-reset journey and returns its continuation token.</summary>
        public static HttpResponseMessage AccountPasswordResetStart(string env, string email)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountPasswordResetStart(email));
        }

        /// <summary>Validates an Entra one-time password and advances the account journey.</summary>
        public static HttpResponseMessage AccountEntraOtpValidation(string env, string token, string otp, ValidationRequestType requestType)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountEntraOtpValidation(token, otp, requestType));
        }

        /// <summary>Submits a new password to complete an Entra account journey.</summary>
        public static HttpResponseMessage AccountEntraPasswordSubmission(string env, string token, string password)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountEntraPasswordSubmission(token, password));
        }

        /// <summary>Invites a Flying Club member to join the authenticated member's household.</summary>
        public static HttpResponseMessage AccountInviteMember(string env, string token, string inviteeFcNumber, string groupName)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountInviteMember(inviteeFcNumber, groupName), headers);
        }

        /// <summary>Lets an invitee accept or decline an invitation, or lets an inviter cancel it.</summary>
        public static HttpResponseMessage AccountInviteMemberAction(string env, string token, string actionType, string inviterFcNumber, string inviteeFcNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountInviteMemberAction(actionType, inviterFcNumber, inviteeFcNumber), headers);
        }

        /// <summary>Exchanges a refresh token for updated account authentication tokens.</summary>
        public static HttpResponseMessage AccountRefreshToken(string env, string token)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountRefreshToken(token));
        }

        /// <summary>Checks a member's personalised campaign eligibility and claim status.</summary>
        public static HttpResponseMessage ValidatePcmEligibility(string env, string? uniqueId)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.ValidatePcmEligibility(uniqueId));
        }

        /// <summary>Registers a member for an American Express campaign.</summary>
        public static HttpResponseMessage AmexRegisterCampaign(string env, string token, string uniqueId, string assignedBy)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AmexRegisterCampaign(uniqueId, assignedBy), headers);
        }

        /// <summary>Updates the authenticated member's marketing and partner preferences.</summary>
        public static HttpResponseMessage UpdatePreference(string env, string token, UpdatePreference request)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                {"Authorization", $"Bearer {token}"}
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.UpdatePreference(request), headers);
        }

        /// <summary>Generates an Entra one-time password for an email address.</summary>
        public static HttpResponseMessage AccountEntraOTPGenerate(string env, string emailId)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountEntraOTPGenerate(emailId));
        }

        /// <summary>Enrols a new Flying Club member through the Entra account journey.</summary>
        public static HttpResponseMessage AccountEntraEnrolMember(string env, AccountEntraEnrolMemberRequest enrolmentRequest)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountEntraEnrolMember(enrolmentRequest));
        }

        /// <summary>Creates a Young Flyer account managed by the authenticated parent member.</summary>
        public static HttpResponseMessage AccountCreateYoungFlyerAccount(string env, string token, string firstName, string lastName, string dateOfBirth, string gender, string channelId)
        {
            System.Collections.Specialized.NameValueCollection headers = new();

            if (!token.IsNullOrWhiteSpace())
            {
                // Omit the header for authentication-negative tests so GraphQL receives no parent access token.
                headers.Add("Authorization", $"Bearer {token}");
            }

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.AccountCreateYoungFlyerAccount(firstName, lastName, dateOfBirth, gender, channelId), headers);
        }

        /// <summary>Claims Virgin Atlantic points for a ticket and returns the credited flight segments.</summary>
        public static HttpResponseMessage ClaimVSPoints(string env, string token, string? ticketNumber, string? fcNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", $"Bearer {token}" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.ClaimVSPoints(ticketNumber, fcNumber), headers);
        }

        /// <summary>Claims points for a partner flight and returns the credited flight segments.</summary>
        public static HttpResponseMessage ClaimPartnerFlightPoints(string env, string token, JObject request)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", $"Bearer {token}" }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"), Factory.GraphQL.ClaimPartnerFlightPoints(request), headers);
        }
    }
}
