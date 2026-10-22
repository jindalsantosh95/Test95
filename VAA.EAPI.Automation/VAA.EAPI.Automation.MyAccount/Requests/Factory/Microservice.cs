using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MyAccount.Requests.Factory
{
    public static class Microservice
    {
        private static Regex postCodeRegex = new(@"(postOrZipCode="")([^""]*)("")", RegexOptions.Compiled);
        private static Regex countryCodeRegex = new(@"(country="")([^""]*)("")", RegexOptions.Compiled);
        private static Regex streetDetailsRegex = new(@"(streetDetails="")([^""]*)("")", RegexOptions.Compiled);

        public static string EnrolmentLoyaltyUpgrade(string firstName, string lastName, string emailId)
        {
            string payload = File.ReadAllText("Requests/Body/Microservices/LoyalityUpgrade.json");
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);
            payloadObject["customerDetails"]["firstName"] = firstName;
            payloadObject["customerDetails"]["lastName"] = lastName;
            payloadObject["customerDetails"]["title"] = "Dr";
            payloadObject["customerDetails"]["contactDetails"]["emailDetails"][0]["emailId"] = emailId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string Enrolment(string scenario)
        {
            string payload = File.ReadAllText("Requests/Body/Microservices/Enrolment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject.ApplyMapping($"Tests/Data/Mapping.{scenario}.csv");

            return JsonConvert.SerializeObject(payloadObject);
        }
    
        public static string Enrolment(string firstName, string lastName, string emailId, string postCode, string country, ITestOutputHelper? output,
            string customerType, string dateOfBirth, string city, string addressVerified, string addressType, string addressLine1, string phoneType, string memberType, bool guardianTermsAndConditionsAccepted, string channelId, string title, string gender, string preferredPartnerFFP, string phoneCountryCode, string region)
        {
            string payload = File.ReadAllText("Requests/Body/Enrolment/Enrolment.json");
            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);
            payloadObject["customerDetails"]["firstName"] = firstName;
            payloadObject["customerDetails"]["lastName"] = lastName;
            payloadObject["customerDetails"]["gender"] = gender;
            if (channelId == "POWERAPP")
                payloadObject["customerDetails"]["title"] = title;
            payloadObject["customerDetails"]["customerType"] = customerType;
            payloadObject["customerDetails"]["dateOfBirth"] = dateOfBirth;
            payloadObject["customerDetails"]["contactDetails"]["emailDetails"][0]["emailId"] = emailId;
            payloadObject["customerDetails"]["addressDetails"][0]["addressType"] = addressType;
            payloadObject["customerDetails"]["addressDetails"][0]["addressLine1"] = addressLine1;
            payloadObject["customerDetails"]["addressDetails"][0]["city"] = city;
            payloadObject["customerDetails"]["addressDetails"][0]["addressVerified"] = addressVerified;
            payloadObject["customerDetails"]["addressDetails"][0]["postalCode"] = postCode;
            payloadObject["customerDetails"]["addressDetails"][0]["country"] = country;
            // Map the supplied regional values so the Facade enrolment record matches the selected address.
            payloadObject["customerDetails"]["addressDetails"][0]["region"] = region;
            payloadObject["customerDetails"]["contactDetails"]["phoneDetails"][0]["phoneType"] = phoneType;
            payloadObject["customerDetails"]["contactDetails"]["phoneDetails"][0]["countryCode"] = phoneCountryCode;
            payloadObject["customerDetails"]["contactDetails"]["phoneDetails"][0]["phoneNumber"] = $"{DataGenerator.RandomNumber(11111, 99999, true)}{DataGenerator.RandomNumber(11111, 99999, true)}";
            payloadObject["customerDetails"]["loyaltyMember"]["memberType"] = memberType;
            payloadObject["customerDetails"]["loyaltyMember"]["guardianDetails"]["guardianTermsAndConditionsAccepted"] = guardianTermsAndConditionsAccepted;
            payloadObject["customerDetails"]["preferences"].FirstOrDefault(x => (string)x["preferenceType"] == "PreferredPartnerFFP")["preferenceValue"] = preferredPartnerFFP;

            return JsonConvert.SerializeObject(payloadObject);
        }
        
        public static string AddressLookUp(string countryCode, string postCode, string streetName)
        {
            string payload = File.ReadAllText(@"Requests/Body/AddressLookUp/AddressLookUp.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["CountryCode"] = countryCode;
            payloadObject["PostCode"] = postCode;
            payloadObject["AddressLine1"] = streetName;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string WesalAddressLookUp(string countryCode, string postCode, string streetName)
        {
            string payload = File.ReadAllText(@"Requests/Body/AddressLookUp/AdressLookUpWesalService.xml");

            payload = postCodeRegex.Replace(payload, $"${{1}}{postCode}${{3}}");

            payload = countryCodeRegex.Replace(payload, $"${{1}}{countryCode}${{3}}");

            payload = streetDetailsRegex.Replace(payload, $"${{1}}{streetName}${{3}}");

            return payload;
        }

        public static string VredProfile(string emailId)
        {
            string payload = File.ReadAllText(@"Requests/Body/Enrolment/VRedProfile.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["email"] = emailId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GenerateOtp(string emailId)
        {
            string payload = File.ReadAllText(@"Requests/Body/Enrolment/OtpGeneration.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["emailId"] = emailId;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string ValidateOtp(string emailId, int otp)
        {
            string payload = File.ReadAllText(@"Requests/Body/Enrolment/ValidateOtp.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["emailId"] = emailId;
            payloadObject["otp"] = otp;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string GetBookingDetails(string source, string destination, string bookingClass)
        {
            string payload = File.ReadAllText(@"Requests/Body/Microservices/BookingDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["flownDetails"]["OriginCountryCode"] = source;
            payloadObject["flownDetails"]["DestinationCountryCode"] = destination;
            payloadObject["flownDetails"]["OriginalBookedClass"] = bookingClass;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string SkyTeamClaimEligibility(string fcNumber, string firstName, string lastName, string flightNumber, string origin, string destination, string ticketNumber)
        {
            string payload = File.ReadAllText(@"Requests/Body/Microservices/SkyTeamClaimDetails.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["ticketInfo"]["fFPAccountNumber"] = fcNumber;
            payloadObject["ticketInfo"]["passengerFirstName"] = firstName;
            payloadObject["ticketInfo"]["passengerLastName"] = lastName;
            payloadObject["ticketInfo"]["operatingFlightNumber"] = flightNumber;
            payloadObject["ticketInfo"]["flightSegmentOrigin"] = origin;
            payloadObject["ticketInfo"]["flightSegmentDestination"] = destination;
            payloadObject["ticketInfo"]["eticketNumber"] = ticketNumber;

            return JsonConvert.SerializeObject(payloadObject);
        }

        public static string AutomatchingMS(string emailId, string customerid, string memberNumber)
        {
            string payload = File.ReadAllText(@"Requests/Body/Microservices/Automatching.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            payloadObject["email"] = emailId;
            payloadObject["customerId"] = customerid;
            payloadObject["memberNumber"] = memberNumber;


            return JsonConvert.SerializeObject(payloadObject);
        }

    }
}
