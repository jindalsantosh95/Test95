using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Operations.Interfaces;
using VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Operations.Services
{
    public sealed class PassengerManifestProvider_SCVv7 : IPassengerManifestProvider
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public PassengerManifestProvider_SCVv7(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<Models.PassengerManifest.PassengerManifestResponse> Query(PassengerManifestRequest request)
        {
            // Construct Url for the request
            string uri = ConfigReader.Get(Constants.Environment.OperationsPassengerManifestUrl)
                            .Replace("{carriercode}", request.Carrier)
                            .Replace("{fltno}", request.FlightNumber.ToString())
                            .Replace("{fltdte}", request.FlightDate)
                            .Replace("{origin}", request.Origin);

            // Get the response
            var response = await _connector.GetAsync<DTOs.PassengerManifest_SCVv7.SCVv7_API_Response>(uri, GetHeaders(), await GetToken());

            return (PassengerManifestResponse)response.Response;
        }

        private async Task<string> GetToken() => await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.OperationsPassengerManifestAppId));
        private Dictionary<string, string> GetHeaders() => new Dictionary<string, string>()
        {
            { Common.Constants.Application.AcceptHeader, ConfigReader.Get(Constants.Environment.OperationsPassengerManifestAcceptHeader) },
            { Common.Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.OperationsPassengerManifestOcpKey) }
        };
    }
}
