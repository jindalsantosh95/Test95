using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.MyAccount.Helpers
{
    public static class AuthenticationResponseValidator
    {
        public static bool ValidateUnauthorizedResponse(HttpResponseMessage response, JObject responseBody, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            string expectedErrorMessage = "Authorization token is missing/invalid";

            if (ConfigReader.Get("graphQlMyAccountPublic").Has("mobile"))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    result.Message.Add($"Expected error status code is invalid. Expected 200 OK and returned: {response.StatusCode}");

                string mobileErrorCode = responseBody.RequireToken("errors[0].extensions.code").ToString();
                string mobileErrorMessage = responseBody.RequireToken("errors[0].message").ToString();

                if (!mobileErrorCode.Equals("401"))
                    result.Message.Add($"Expected error code is invalid. Expected 401 and returned: {mobileErrorCode}");

                if (!mobileErrorMessage.Equals(expectedErrorMessage))
                    result.Message.Add($"Expected error message is invalid. Expected {expectedErrorMessage} and returned: {mobileErrorMessage}");

                return result.IsValid;
            }

            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                result.Message.Add($"Expected error status code is invalid. Expected 401 OK and returned: {response.StatusCode}");

            string errorCode = responseBody.RequireToken("errors[0].extensions.code").ToString();
            string errorMessage = responseBody.RequireToken("errors[0].message").ToString();

            if (!errorCode.Equals("401"))
                result.Message.Add($"Expected error code is invalid. Expected 401 and returned: {errorCode}");

            if (!errorMessage.Equals(expectedErrorMessage))
                result.Message.Add($"Expected error message is invalid. Expected {expectedErrorMessage} and returned: {errorMessage}");


            return result.IsValid;
        }
    }
}
