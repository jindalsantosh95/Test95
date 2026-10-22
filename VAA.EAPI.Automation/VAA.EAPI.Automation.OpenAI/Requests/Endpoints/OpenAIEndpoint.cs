using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;


namespace VAA.EAPI.Automation.OpenAI.Requests.Endpoints
{
    public static class OpenAIEndpoint
    {
        public static readonly string _baseUrl = Core.Helpers.ConfigReader.Get("BaseUrl");

        public static HttpResponseMessage OpenAIPost(string payload, string token)
        {
            var headers = new System.Collections.Specialized.NameValueCollection{};

            if (!token.IsNullOrEmpty())
            {
                headers.Add("Authorization", $"Bearer {token}");
            }

            return WebRequest.Json.Post($"{_baseUrl}", payload, headers);
        }
    }
}
