using VAA.EAPI.Automation.Core;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MyAccount.Requests.Endpoint
{
    public static class Microservice
    {
        private static readonly string _env = Config.Environment;

        public static HttpResponseMessage Enrolment(string env, string bearer, string channelID, string firstName, string lastName, string emailId, string postCode, string country, ITestOutputHelper? output,
           string customerType = "Loyalty", string dateOfBirth = "1997-11-01", string city = "Tel Aviv", string addressVerified = "true", string addressType = "Work", string addressLine1 = "104 Test address1", string phoneType = "Mobile", string memberType = "Individual", bool guardianTermsAndConditionsAccepted = true, string title = "Mr", string channelUserId = "", string gender = "", string preferredPartnerFFP = "", string phoneCountryCode = "44", string region = "NY")
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-EnrolmentFacade-RetrieveEnrolmentOcpKey") },
                { "channelId", channelID }
            };
            if (channelID == "POWERAPP")
                headers.Add("channelUserId", string.IsNullOrEmpty(channelUserId) ? "456321" : channelUserId);

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{((env.ToLower() == "uat") ? string.Empty : $"{env}/")}RunTheOperation/CustomerManagement/CustomerIdentityAndAccessManagement/customers/enrolment"), Factory.Microservice.Enrolment(firstName, lastName, emailId, postCode, country, output,
                customerType, dateOfBirth, city, addressVerified, addressType, addressLine1, phoneType, memberType, guardianTermsAndConditionsAccepted, channelID, title, gender, preferredPartnerFFP, phoneCountryCode, region), headers);
        }

        public static HttpResponseMessage EnrolmentLoyaltyUpgrade(string env, string bearer, string channelId, string firstName, string lastName, string emailId)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer }
            };

            if (channelId == "POWERAPP")
                headers.Add("channelUserId", "456321");
            headers.Add("Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-EnrolmentFacade-RetrieveEnrolmentOcpKey"));
            headers.Add("channelId", channelId);

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{((env.ToLower() == "uat") ? string.Empty : $"{env}/")}RunTheOperation/CustomerManagement/CustomerIdentityAndAccessManagement/customers/enrolment"),
                Factory.Microservice.EnrolmentLoyaltyUpgrade(firstName, lastName, emailId), headers);
        }

        public static HttpResponseMessage AddressLookUp(string env, string bearer, string countryCode, string postCode, string streetName)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-Ocp-Apim-Subscription-Key-AddressLookUp") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/addresslookupmicroservice/AddressDetailsV2"),
                Factory.Microservice.AddressLookUp(countryCode, postCode, streetName), headers);
        }

        public static HttpResponseMessage AddressDetailsV2(string env, string bearer, string countryCode, string postCode, string streetName)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-Ocp-Apim-Subscription-Key-AddressLookUp") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/addresslookupmicroservice/AddressDetailsV2"),
                Factory.Microservice.AddressLookUp(countryCode, postCode, streetName), headers);
        }

        public static HttpResponseMessage AddressLookUpWesalEndpoint(string countryCode, string postCode, string streetName)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Content-Type", "text/xml" },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-Ocp-Apim-Subscription-Key-WesalService") }
            };

            return WebRequest.Xml.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/wesal"),
                Factory.Microservice.WesalAddressLookUp(countryCode, postCode, streetName), headers);
        }

        public static HttpResponseMessage AzureB2C(string bearer, string flynigClubCardNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer }
            };

            return WebRequest.Get(Config.AzureB2C.GetUrl($"v1.0/users?$select=Identities,givenName,surname,id,objectId,userPrincipalName,accountEnabled,extension_b4e6641f44c644ea8b9a899e786c7579_MemberType,extension_b4e6641f44c644ea8b9a899e786c7579_memberStatus&$filter=Identities/any(x:x/issuer eq 'avateunloyaltyfcb2c.onmicrosoft.com' and issuerAssignedId eq '{flynigClubCardNumber}')"), headers);
        }

        public static HttpResponseMessage Vred(string bearer, string flynigClubCardNumber)
        {
            string apimSubscriptionKey = Vault.Default.GetSecretValue("MYA-SubscriptionKeyVred");

            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                //Need to check where we will put this APIM Key
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey },
                { "Content-Type", "application/json" }
            };

            return WebRequest.Get(Config.VredEndPoint.GetUrl($"/members/members/v1?partnerUserId={flynigClubCardNumber}&requestId=123"), headers);
        }

        public static HttpResponseMessage VredProfile(string bearer, string emailId)
        {
            string apimSubscriptionKey = ConfigReader.Get("Ocp-Apim-Subscription-Key_VRed");

            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                //Need to check where we will put this APIM Key
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey },
                { "Content-Type", "application/json" }
            };

            return WebRequest.Json.Post(Config.VredProfileEndPoint.GetUrl("/v1/profiles/search"), Factory.Microservice.VredProfile(emailId), headers);
        }

        public static HttpResponseMessage VredPoints(string bearer, string flynigClubCardNumber)
        {
            string apimSubscriptionKey = ConfigReader.Get("Ocp-Apim-Subscription-Key_VRed");

            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                //Need to check where we will put this APIM Key
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey },
                { "Content-Type", "application/json" }
            };

            return WebRequest.Get(Config.VredEndPoint.GetUrl($"/balances/v1?memberId={flynigClubCardNumber}"), headers);
        }

        public static HttpResponseMessage VredPointsMsEndPoint(string env, string bearer, string flyingClubCardNumber)
        {
            string apimSubscriptionKey = Vault.Default.GetSecretValue("MYA-Ocp-Apim-Subscription-Key-AddressLookUp");

            System.Collections.Specialized.NameValueCollection headers = new()
            {
                 { "Authorization", bearer },
                //Need to check where we will put this APIM Key
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey }
            };

            return WebRequest.Get(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}/digital/vred/{flyingClubCardNumber}/VirginPoints"), headers);
        }

        public static HttpResponseMessage VRedMSCardLinks(string env, string bearer, string flyingClubCardNumber)
        {
            string apimSubscriptionKey = Vault.Default.GetSecretValue("MYA-Ocp-Apim-Subscription-Key-AddressLookUp");

            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", apimSubscriptionKey }
            };

            return WebRequest.Get(Config.DigitalTestVirginAtlantic.GetUrl($"/{env}/digital/vred/VredCardLinks/{flyingClubCardNumber}"), headers);
        }

        public static HttpResponseMessage GenerateOtp(string env, string bearer, string emailId)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-EnrolmentFacade-RetrieveEnrolmentOcpKey") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/identitymicroservice/OTPGenerate"),
                Factory.Microservice.GenerateOtp(emailId), headers);
        }

        public static HttpResponseMessage ValidateOtp(string env, string bearer, string emailId, int otp)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-EnrolmentFacade-RetrieveEnrolmentOcpKey") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/identitymicroservice/ValidateOTP"),
                Factory.Microservice.ValidateOtp(emailId, otp), headers);
        }

        public static HttpResponseMessage GetSegmentDistance(string env, string source, string destination)
        {
            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/referencemicroservice/getsegmentdistance/{source}/{destination}"));
        }

        public static HttpResponseMessage GetBookingDetails(string env, string source, string destination, string bookingClass)
        {
            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/referencemicroservice/getbookingdetails"), Factory.Microservice.GetBookingDetails(source, destination, bookingClass));
        }

        public static HttpResponseMessage SkyTeamClaimEligibility(string env, string bearer, string fcNumber, string firstName, string lastName, string flightNumber, string origin, string destination, string ticketNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", Vault.Default.GetSecretValue("MYA-SkyMs-RetrieveSkyTeamMsOcpKey") }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/skyms/checkclaimeligibility"), Factory.Microservice.SkyTeamClaimEligibility(fcNumber, firstName, lastName, flightNumber, origin, destination, ticketNumber), headers);
        }

        public static HttpResponseMessage GetCountries(string env)
        {
            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/referencemicroservice/countries"));
        }

        public static HttpResponseMessage CreateVAEnrolment(string bearer, string ocpSubKey, string firstName, string lastName, string middleName, string phoneNumber, string customerType, string doB, string emailId)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/Microservices/VAEnrolment"), VAA.EAPI.Automation.Core.Helpers.Requests.Factory.CreateVAEnrolment(firstName, lastName, middleName, phoneNumber, customerType, doB, emailId), headers);
        }

        public static HttpResponseMessage GetVAEnrolment(string bearer, string ocpSubKey, string responseUrl)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };
            return WebRequest.Get(responseUrl, headers);
        }

        public static HttpResponseMessage AutomatchingCustomerMS(string env, string bearer, string ocpSubKey, string email, string customerid, string memberNumber)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
            {
                { "Authorization", bearer },
                { "Ocp-Apim-Subscription-Key", ocpSubKey }
            };

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}/microservices/customerProfileSearch"),
                Factory.Microservice.AutomatchingMS(email, customerid, memberNumber), headers);
        }

        public static HttpResponseMessage RetrieveVouchers(string bearer, string ocpSubKey, string customerId)
        {
            System.Collections.Specialized.NameValueCollection headers = new()
                {
                    { "Authorization", bearer },
                    { "Ocp-Apim-Subscription-Key", ocpSubKey }
                };

            return WebRequest.Get(Config.TestResourcesVirginAtlantic.GetUrl($"/{_env}/customertransactions/{customerId}/Vouchers"), headers);
        }
    }
}
