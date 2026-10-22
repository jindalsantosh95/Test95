using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.FlightDetails;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service
{
    public class RetroApiServices : IRetroApiService
    {
        private readonly string _clientIDFlightAPI = ConfigReader.Get(Constants.Environment.clientIDFlightAPI);

        private readonly string _clientSecrectFlightAPI = ConfigReader.Get(Constants.Environment.clientSecrectFlightAPI);

        private readonly string _codeShareFlightApiUrl = ConfigReader.Get(Constants.Environment.codeShareFlightApiUrl);

        private static readonly string _resourceIDMemberApi = ConfigReader.Get(Constants.Environment.resourceIDMemberApi);

        private static readonly string _subscriptionKeyMemberApi = ConfigReader.Get(Constants.Environment.subscriptionKeyMemberApi);

        private static readonly string _authUrlApi = ConfigReader.Get(Constants.Environment.authUrlApi);

        private readonly IHttpConnector _httpConnector;

        private static readonly string _tenantNameApi = ConfigReader.Get(Constants.Environment.tenantNameApi);

        public RetroApiServices(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector;
        }

        public async Task<FlightDetails> GetCodeShareFlightDetails(string flightNumber, string activityDate)
        {
            string token = await GetAuthToken(_clientIDFlightAPI, _clientSecrectFlightAPI, _resourceIDMemberApi);

            string codeShareFlightApi = _codeShareFlightApiUrl + $"Airline_Code=ALL&Local_Date_From={activityDate}&Local_Date_To={activityDate}&Flight_Number={flightNumber}";

            KeyValuePair<bool, string> response = await GetAPIResponse("Get", codeShareFlightApi, token, null, _subscriptionKeyMemberApi);

            if (response.Key)
            {
                var apiResponse = JsonConvert.DeserializeObject<FlightDetails>(response.Value);

                return apiResponse;
            }

            if(!response.Key && !string.IsNullOrEmpty(response.Value) && response.Value.Contains("not found", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            throw (new ApiFailedException("Flight-CodeShare", new Exception(response.Value)));
        }

        private async Task<string> GetAuthToken(string clientId, string clientSecret, string resourceId)
        {
            string authString = _authUrlApi + _tenantNameApi; //"https://login.microsoftonline.com/" + "6bdfd5a8-531f-4f14-9af1-dc7b444c3ad8";

            AuthenticationContext authenticationContext = new AuthenticationContext(authString, false);

            ClientCredential clientCredential = new ClientCredential(clientId, clientSecret);

            AuthenticationResult result = await authenticationContext.AcquireTokenAsync(resourceId, clientCredential).ConfigureAwait(false);

            return result == null ? throw new InvalidOperationException("Failed to obtain the JWT token") : (result?.AccessToken);
        }

        private async Task<KeyValuePair<bool, string>> GetAPIResponse(string method, string url, string bearerToken, string data, string ocpApimSubscriptionKey)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), url);

            request.Headers.Accept.Clear();

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

            request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true };

            if (!string.IsNullOrEmpty(ocpApimSubscriptionKey))
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", ocpApimSubscriptionKey);
            }

            if (!string.IsNullOrEmpty(bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }

            switch (method)
            {
                case "Post":

                    request.Content = new StringContent(data, Encoding.UTF8, "application/json");

                    break;
            }
                
            response = await _httpConnector.SendAsync(request, new CancellationToken());            

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();

                return new KeyValuePair<bool, string>(false, error);
            }

            string result = await response.Content.ReadAsStringAsync();

            return new KeyValuePair<bool, string>(true, result);
        }
    }
}
