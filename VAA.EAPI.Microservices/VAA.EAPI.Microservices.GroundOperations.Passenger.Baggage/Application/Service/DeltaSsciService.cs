using System.Text;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Infrastructure.Helper.Interface;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Service
{
    public class DeltaSsciService : IDeltaSsciService
    {
        public readonly IHttpConnector _httpConnector;
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

        private async Task<string> GetToken() => await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Application.DeltaClientIdValue), ConfigReader.Get(Constants.Application.DeltaClientSecret));

        public async Task<BagsData> GetBaggageInfoAsync(string travelPartyId)
        {
            var token = await GetToken();
            return await _httpConnector.GetAsync<BagsData>($"{_serviceEndpoint}/travelparties/{travelPartyId}/addBags", _headers, token);
        }

        public async Task<AddBagsResponse> SaveBaggageAsync(string travelPartyId, UpdateBagsRequest request)
        {
            var content = new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json");
            var token = await GetToken();

            return await _httpConnector.PostAsync<AddBagsResponse>($"{_serviceEndpoint}/travelparties/{travelPartyId}/addBags", content, _headers, token);
        }

        public async Task<AddBagsViewData> CheckPaymentAsync(string travelPartyId, UpdateBagsRequest request)
        {
            var content = new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json");
            var token = await GetToken();

            return await _httpConnector.PatchAsync<AddBagsViewData>($"{_serviceEndpoint}/travelparties/{travelPartyId}/addBags", content, _headers, token);
        }

        public async Task<BagsPurchaseReceipt> ConfirmBaggageAsync(string travelPartyId, PurchaseRequest request)
        {
            var content = new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json");
            var token = await GetToken();

            return await _httpConnector.PostAsync<BagsPurchaseReceipt>($"{_serviceEndpoint}/travelparties/{travelPartyId}/reviewAndPayDetail", content, _headers, token);
        }
    }
}
