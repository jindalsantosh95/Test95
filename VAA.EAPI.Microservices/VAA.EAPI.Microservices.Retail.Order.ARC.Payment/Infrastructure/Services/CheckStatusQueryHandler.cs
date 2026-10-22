using CSharpFunctionalExtensions;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Services
{
    public class CheckStatusQueryHandler : ICheckStatusQuery
    {
        private readonly IHttpConnector _httpConnector;
        public CheckStatusQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Result<Response<CheckResponse>>> Handle(string agencyNumber)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigReader.Get(Constants.Environment.ArcCheckClientId)}:{ConfigReader.Get(Constants.Environment.ArcCheckClientSecret)}"));

            var tokenHeaders = new Dictionary<string, string>()
                {
                    {"Authorization", $"Basic {credentials}"}
                };

            var tokenRequestBody = new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials" },
                { "scope", ConfigReader.Get(Constants.Environment.ArcCheckScope) }
            };

            var tokenUrl = ConfigReader.Get(Constants.Environment.ArcCheckTokenUrl);

            var token = await _httpConnector.SendAsync(tokenUrl, tokenHeaders, string.Empty, HttpMethod.Post, new FormUrlEncodedContent(tokenRequestBody));

            if (!token.IsSuccessStatusCode)
                return new Response<CheckResponse>() { Status = EAPI.Domain.Entities.Common.Status.OK, Value = new CheckResponse() { Message = "TokenGenerationFailed" } };

            var tokenResponseContent = await token.Content.ReadAsStringAsync();
            var tokenObj = JsonSerializer.Deserialize<JsonElement>(tokenResponseContent);

            if (tokenObj.TryGetProperty("access_token", out var accessTokenElement))
            {
                var accessToken = accessTokenElement.GetString();

                var headers = new Dictionary<string, string>()
                    {
                        { "Authorization", $"Bearer {accessToken}" },
                    };

                NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                queryString.Add("agencynumber", agencyNumber);

                var requestUrl = $"{ConfigReader.Get(Constants.Environment.ArcCheckBaseUrl)}?{queryString}";

                var httpResponseMessage = await _httpConnector.SendAsync(requestUrl, headers, string.Empty, HttpMethod.Get, string.Empty);

                var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                var checkResponse = Json.Deserialize<CheckResponse>(responseContent);

                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<CheckResponse>() { Status = EAPI.Domain.Entities.Common.Status.OK, Value = checkResponse };
                }
                else
                    return new Response<CheckResponse>() { Status = EAPI.Domain.Entities.Common.Status.BadRequest, Value = checkResponse }; ;
            }
            else
            {
                return new Response<CheckResponse>() { Status = EAPI.Domain.Entities.Common.Status.OK, Value = new CheckResponse() { Message = "AccessTokenNotFoundInResponse" } };
            }
        }

    }
}
