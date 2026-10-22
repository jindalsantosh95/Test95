using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.PaxOps.Helper;
using VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS;
using VAA.EAPI.GraphQL.Domain.PaxOps.Interfaces;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Services
{
    public class BaggageServiceProvider(IHttpConnector httpConnector) : IBaggageServiceProvider
    {
        protected readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private static readonly Dictionary<string, string> _headers = new() { { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.BaggageMsOcpKey) } };

        private static readonly string _baggageMsUrl = ConfigReader.Get(Common.Constants.Environment.BaggageMSUrl);

        public async Task<BaggageResponse> GetBaggageAsync(string checkInId, bool? hidePaidBaggage)
        {
            string uri = $"{_baggageMsUrl}{Common.Constants.Application.GetBaggage}/{await CheckInIdHelper.GetTravelPartyId(checkInId)}?hidePaidBaggage=" + (hidePaidBaggage == true ? "true" : "false");

            return (await _httpConnector.SendAsync<BaggageResponse>(uri, _headers, string.Empty, HttpMethod.Get, string.Empty)).Response;
        }

        public async Task<SaveBaggageResponse> SaveBaggageAsync(Models.Baggage.SaveBaggageServiceRequest request)
        {
            string uri = $"{_baggageMsUrl}{Common.Constants.Application.SaveBaggage}";

            return (await _httpConnector.SendAsync<SaveBaggageResponse>(uri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<PaymentEligibilityCheckResponse> PaymentEligibilityCheckAsync(Models.Baggage.PaymentCheckServiceRequest request)
        {
            string uri = $"{_baggageMsUrl}{Common.Constants.Application.PaymentEligibilityCheck}";

            return (await _httpConnector.SendAsync<PaymentEligibilityCheckResponse>(uri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<ConfirmBaggageResponse> ReviewPaymentDetails(Models.Baggage.ReviewPayDetailsRequest request)
        {
            string uri = $"{_baggageMsUrl}{Common.Constants.Application.ConfirmBaggage}";

            return (await _httpConnector.SendAsync<ConfirmBaggageResponse>(uri, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;

        }

    }
}
