using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerDataManagement;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Services
{
    public class PartnerDataManagementQueryProvider : IPartnerDataManagementQueryProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public PartnerDataManagementQueryProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<VSFlownDetailsResponse> GetVSRetroFlownData(GetVSRetroFlownDataRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.PdmMSAppId));

            var headers = new Dictionary<string, string>()
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CommonEAPIOcpAPIMKey) }
            };

            var uri = $"{ConfigReader.Get(Common.Constants.Environment.PdmMSUrl)}{Common.Constants.Application.GetFlownDataEndpoint}";

            var response = (await _connector.SendAsync<VSFlownDetailsResponse>(uri, headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
            return new VSFlownDetailsResponse
            {
                VSRetroFlownDetails = response?.VSRetroFlownDetails ?? new List<VSRetroFlownDetail>()
            };
        }

        public async Task<string> UpdateVSRetroFlownStatus(TicketStatusRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.PdmMSAppId));
            var headers = new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CommonEAPIOcpAPIMKey) }
            };

            var uri = $"{ConfigReader.Get(Common.Constants.Environment.PdmMSUrl)}{Common.Constants.Application.UpdateTicketStatusEndpoint}";
            return (await _connector.SendAsync<string>(uri, headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<EditVSRetroFlownDataResponse> UpdateVSRetroFlownDetails(EditVSRetroFlownDataRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.PdmMSAppId));
            var headers = new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CommonEAPIOcpAPIMKey) }
            };

            var uri = $"{ConfigReader.Get(Common.Constants.Environment.PdmMSUrl)}{Common.Constants.Application.UpdateVSRetroFlownDetailsEndpoint}";
            return (await _connector.SendAsync<EditVSRetroFlownDataResponse>(uri, headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
        }

        public async Task<ValidateVSClaimInputsResponse> ValidateVSClaimInputs(ValidateVSClaimInputsRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Common.Constants.Environment.PdmMSAppId));
            var headers = new Dictionary<string, string>
            {
                { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Common.Constants.Environment.CommonEAPIOcpAPIMKey) }
            };

            var uri = $"{ConfigReader.Get(Common.Constants.Environment.PdmMSUrl)}{Common.Constants.Application.ValidateVSClaimInputsEndpoint}";
            return (await _connector.SendAsync<ValidateVSClaimInputsResponse>(uri, headers, token, HttpMethod.Post, Json.Serialize(request))).Response;
        }
    }
}
