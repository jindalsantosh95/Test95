using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.MyAccount.Requests.Factory
{
    public class IdentityFacadeApi
    {
        public static string ResetPassword(string fcNo, string password)
        {
            string payload = File.ReadAllText("Requests/Body/IdentityFacadeLayer/ResetPassword.json");

            JObject payloadObject = (JObject)JsonConvert.DeserializeObject<dynamic>(payload);
            JToken variables = payloadObject["ListOfVAMemberUpdate"]["LOYMember"];

            variables["MemberNumber"] = fcNo;
            variables["ListOfContact"]["Contact"]["Password"] = password;

            return JsonConvert.SerializeObject(payloadObject);
        }
    }
}
