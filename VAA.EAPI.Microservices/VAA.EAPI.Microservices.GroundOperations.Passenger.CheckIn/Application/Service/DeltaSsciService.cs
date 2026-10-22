using System.Text;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.DeltaSsci;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Service
{
    public class DeltaSsciService : IDeltaSsciService
    {
        private readonly IHttpConnector _httpConnector;
        private readonly string _serviceEndpoint;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        private readonly static Dictionary<string, string> _headers = new()
        {
            {Constants.Application.DeltaChannelName, ConfigReader.Get(Constants.Application.DeltaChannelValue)},
            {Constants.Application.DeltaApiId, ConfigReader.Get(Constants.Application.DeltaApiIdValue)},
            {Constants.Application.DeltaClientIdName, ConfigReader.Get(Constants.Application.DeltaClientIdValue)}
        };

        public DeltaSsciService(IHttpConnector httpConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _httpConnector = httpConnector;
            _air4TokenGenerator = air4TokenGenerator;
            _serviceEndpoint = ConfigReader.Get(Constants.Environment.DeltaSsciEndpoint);
        }
        private async Task<string> GetToken() => await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Application.DeltaClientIdValue), ConfigReader.Get(Constants.Application.DeltaTrmClientSecret));


        public async Task<TravelParty> PostTravelPartiesAsync(InputObject inputObject)
        {
            var content = new StringContent(Json.Serialize(inputObject), Encoding.UTF8, "application/json");
            var token = await GetToken();
            return await _httpConnector.PostAsync<TravelParty>($"{_serviceEndpoint}/travelparties", content, _headers, token);
        }

        public async Task<TravelParty> GetTravelPartiesAsync(string travelPartyId)
        {
            var token = await GetToken();
            return await _httpConnector.GetAsync<TravelParty>($"{_serviceEndpoint}/travelparties/{travelPartyId}", _headers, token);
        }

        public async Task<TravelParty> PatchTravelPartiesAsync(string travelPartyId, SelectPassengerRequest selectPassengerRequest)
        {
            var content = new StringContent(Json.Serialize(selectPassengerRequest), Encoding.UTF8, "application/json");
            var token = await GetToken();
            return await _httpConnector.SendAsync<TravelParty>($"{_serviceEndpoint}/travelparties/{travelPartyId}", _headers, token, HttpMethod.Patch, content);
        }

        public async Task<PassportData> GetPassportDetailsAsync(string travelPartyId)
        {
            var token = await GetToken();
            return await _httpConnector.GetAsync<PassportData>($"{_serviceEndpoint}/travelParties/{travelPartyId}/passportdetails", _headers, token);
        }

        public async Task<AddPassportResponse> PostPassportDetailsAsync(string travelPartyId, AddPassportRequest addPassportRequest)
        {
            var content = new StringContent(Json.Serialize(addPassportRequest), Encoding.UTF8, "application/json");
            var token = await GetToken();
            return await _httpConnector.PostAsync<AddPassportResponse>($"{_serviceEndpoint}/travelparties/{travelPartyId}/passportdetails", content, _headers, token);
        }

        public async Task<ContactTracingResponse> SaveContactTracingAsync(string travelPartyId, ContactTracingRequest contactTracingRequest)
        {
            var token = await GetToken();
            return await _httpConnector.PostAsync<ContactTracingResponse>($"{_serviceEndpoint}/travelparties/{travelPartyId}/contactracing", new StringContent(Json.Serialize(contactTracingRequest), Encoding.UTF8, "application/json"), _headers, token);
        }

        public async Task<BoardingDocumentsResponse> PostBoardingDocumentsAsync(string travelPartyId)
        {
            var token = await GetToken();
            return await _httpConnector.PostAsync<BoardingDocumentsResponse>($"{_serviceEndpoint}/travelparties/{travelPartyId}/boardingdocuments", null, _headers, token);
        }

        public async Task<TravelParty> PostSeatsAsync(string travelPartyId, AddSeatRequest seatRequest)
        {
            var content = new StringContent(Json.Serialize(seatRequest), Encoding.UTF8, "application/json");
            var token = await GetToken();
            return await _httpConnector.PostAsync<TravelParty>($"{_serviceEndpoint}/travelparties/{travelPartyId}/seats", content, _headers, token);
        }

        public async Task<bool> PostBuildTransactionSummaryAsync(string travelPartyId, BuildTransactionSummaryRequest buildTransactionSummaryRequest)
        {
            var content = new StringContent(Json.Serialize(buildTransactionSummaryRequest), Encoding.UTF8, "application/json");
            var token = await GetToken();

            using var response = await _httpConnector.PostAsync($"{_serviceEndpoint}/checkin/buildTransactionSummary/{travelPartyId}", content, _headers, token);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            if (responseBody.IsNullOrWhiteSpace())
            {
                return false;
            }

            try
            {
                var transactionSummaryResponse = Json.Deserialize<BuildTransactionSummaryResponse>(responseBody);
                return transactionSummaryResponse?.Status.Is("success") == true;
            }
            catch (Newtonsoft.Json.JsonException)
            {
                return false;
            }
        }
    }
}
