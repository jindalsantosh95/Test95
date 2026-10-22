using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Wallet.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Service
{
    public class AirshipWalletService(IHttpConnector httpConnector) : IAirshipWalletService
    {
        private readonly IHttpConnector _httpConnector = httpConnector;
        private readonly string _airShipFlightEndpoint = ConfigReader.Get(Constants.Environment.AirshipFlightEndpoint);
        private readonly string _airShipBoardingEndpoint = ConfigReader.Get(Constants.Environment.AirshipWalletLinkEndpoint);

        private readonly Dictionary<string, string> _headers = new()
        {
             {Constants.Application.AirshipTokenHeaderName, $"Basic {ConfigReader.Get(Constants.Environment.AirshipTokenValue)}"},
        };

        public async Task<FlightResponse> PostFlightAsync(string flightExtId, FlightRequest flightRequest)
        {
            var content = new StringContent(Json.Serialize(flightRequest, false), Encoding.UTF8, "application/json");

            return await _httpConnector.PostAsync<FlightResponse>($"{_airShipFlightEndpoint}/{flightExtId}", content, _headers, null);
        }

        public async Task<FlightResponse> PatchFlightAsync(string flightExtId, FlightRequest flightRequest)
        {
            var content = new StringContent(Json.Serialize(flightRequest, false), Encoding.UTF8, "application/json");

            return await _httpConnector.PutAsync<FlightResponse>($"{_airShipFlightEndpoint}/{flightExtId}", content, _headers, null);
        }

        public async Task<FlightResponse> GetFlightAsync(string flightExtId)
        {
            return await _httpConnector.GetAsync<Infrastructure.Airship.FlightResponse>($"{_airShipFlightEndpoint}/{flightExtId}", _headers, null);
        }

        public async Task<AirshipBoardingPassResponse> PostBoardingPassAsync(AirshipBoardingPassRequest flightRequest)
        {
            var content = new StringContent(Json.Serialize(flightRequest, false), Encoding.UTF8, "application/json");

            return await _httpConnector.PostAsync<AirshipBoardingPassResponse>(_airShipBoardingEndpoint, content, _headers, null);
        }

    }
}
