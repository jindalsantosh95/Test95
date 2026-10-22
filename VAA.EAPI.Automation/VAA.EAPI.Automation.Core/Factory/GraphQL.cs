using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Factory
{
    public class GraphQL
    {

        public static string Enrolment(string channelId, string firstName, string lastName, string emailId, string postCode, string countryCode, string password, string dateOfBirth)
        {
            string payload = File.ReadAllText(@"Requests\Body\GraphQL\Enrolment.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);

            JToken variables = payloadObject["variables"];

            variables["request"]["channelId"] = channelId;
            variables["request"]["customerDetails"]["firstName"] = firstName;
            variables["request"]["customerDetails"]["lastName"] = lastName;
            variables["request"]["customerDetails"]["contactDetails"]["emailDetails"][0]["emailId"] = emailId;
            variables["request"]["customerDetails"]["contactDetails"]["phoneDetails"][0]["phoneNumber"] = $"{DataGenerator.RandomNumber(11111, 99999, true)}{DataGenerator.RandomNumber(11111, 99999, true)}";
            variables["request"]["customerDetails"]["addressDetails"][0]["postalCode"] = postCode;
            variables["request"]["customerDetails"]["addressDetails"][0]["country"] = countryCode;
            variables["request"]["customerDetails"]["password"] = password;
            variables["request"]["customerDetails"]["dateOfBirth"] = dateOfBirth;

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}
