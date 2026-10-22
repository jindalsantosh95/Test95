using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Services
{
    public class OTPMutationProvider<T> : IOTPMutationProvider<T>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;

        public OTPMutationProvider(IOAuthTokenProvider tokenProvider, IHttpConnector connector)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<T> GenerateOtp(string emailId)
        {
            string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));

            var headers = new Dictionary<string, string>
                {
                    { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey) }
                };

            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.OTPGenerate}", headers, token, HttpMethod.Post, Json.Serialize(new { emailId = emailId }))).Response;
        }

        public async Task<T> GenerateEntraOtp(string emailId)
        {
            string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));

            var headers = new Dictionary<string, string>
                {
                    { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey) }
                };

            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.EntraOTPGenerate}", headers, token, HttpMethod.Post, Json.Serialize(new { username = emailId }))).Response;
        }

        public async Task<T> GenerateOtpContinuationToken(string emailId)
        {
            string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));

            var headers = new Dictionary<string, string>
                {
                    { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey) }
                };

            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.OTPGenerateContinuationToken}", headers, token, HttpMethod.Post, Json.Serialize(new { emailId }))).Response;
        }

        public async Task<T> ValidateOtpToken(string otpValidationToken)
        {
            string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));

            var headers = new Dictionary<string, string>
                {
                    { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey) }
                };

            return (await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.OtpTokenValidation}", headers, token, HttpMethod.Post, Json.Serialize(new { Token = otpValidationToken }))).Response;
        }

        public async Task<T> ValidateOtp(string emailId, int otp)
        {
            string token = await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Environment.IdentityMSAppId));

            var headers = new Dictionary<string, string>
                {
                    { Application.OcpKeyHeader, ConfigReader.Get(Environment.IdentityOcpKey) }
                };


            var apiResult = await _connector.SendAsync<T>($"{ConfigReader.Get(Environment.IdentityMsUrl)}{Application.ValidateOTP}", headers, token, HttpMethod.Post, Json.Serialize(new { emailId = emailId, otp = otp }));
            return apiResult.Response;
        }
    }
}
