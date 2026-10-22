using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.Facade.EntraFacade.Application.Interfaces;
using VAA.Facade.EntraFacade.Domain;

namespace VAA.Facade.EntraFacade.Application.Command
{
    public class CustomAuthExtensionAdaptor : ICustomAuthExtensionAdaptor
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;

        public CustomAuthExtensionAdaptor(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<Response<ResponseObject>> CustomEmailOtp(EntraRequest entraOtpequest, CancellationToken cancellationToken)
        {
            return new Response<ResponseObject>()
            {
                Status = Status.OK,
                Value = await _httpConnector.SendAsync<ResponseObject>($"{ConfigReader.Get(Constants.Environment.IdentityUrl)}{Constants.Application.CustomEmailOTPSendEndpoint}", GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.IdentityAppId)), HttpMethod.Post, Json.Serialize(entraOtpequest)),
                Errors = null
            };
        }

        public async Task<Response<ResponseObject>> CustomToken(EntraRequest entraCode, CancellationToken cancellationToken)
        {
            return new Response<ResponseObject>()
            {
                Status = Status.OK,
                Value = await _httpConnector.SendAsync<ResponseObject>($"{ConfigReader.Get(Constants.Environment.IdentityUrl)}{Constants.Application.CustomeTokenEndpoint}", GetHeaders(), await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.IdentityAppId)), HttpMethod.Post, Json.Serialize(entraCode)),
                Errors = null
            };
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                {  Constants.Application.OcpKeyHeader , ConfigReader.Get(Constants.Environment.IdentityOcpKey) }
            };
        }
    }
}