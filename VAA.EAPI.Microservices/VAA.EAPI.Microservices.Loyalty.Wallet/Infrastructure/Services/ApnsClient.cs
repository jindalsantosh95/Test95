using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.Apns;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Services
{
    public class ApnsClient : IApnsClient
    {
        private readonly CngKey _key;
        private readonly string _keyId;
        private readonly string _teamId;
        private readonly IHttpConnector _httpConnector;
        private readonly string _bundleId;

        string _jwt;
        DateTime _lastJwtGenerationTime;

        readonly bool _useCert;

        public ApnsClient(IHttpConnector httpConnector, CngKey key, string keyId, string teamId, string bundleId)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _keyId = keyId ?? throw new ArgumentNullException(nameof(keyId));
            _teamId = teamId ?? throw new ArgumentNullException(nameof(teamId));
            _bundleId = bundleId ?? throw new ArgumentNullException(nameof(bundleId));
        }

        public static ApnsClient CreateUsingJwt(IHttpConnector httpConnector, ApnsJwtOptions options)
        {
            return new ApnsClient(httpConnector, options.CertContent, options.KeyId, options.TeamId, options.BundleId);
        }

        public async Task<ApnsResponse> SendAsync(ApplePush push, CancellationToken ct = default)
        {
            var payload = push.GeneratePayload();
            var url = BuildUrl(push);
            var req = CreateHttpRequestMessage(push, payload, url);

            HttpResponseMessage httpResponse;

            httpResponse = await _httpConnector.SendAsync(req, ct).ConfigureAwait(false);
            var responseMessage = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorPayload = Json.Deserialize<ApnsErrorResponsePayload>(responseMessage);

                return ApnsResponse.Error(errorPayload.Reason, errorPayload.ReasonRaw);
            }

            return ApnsResponse.Successful();
        }

        private string BuildUrl(ApplePush push)
        {
            string endpoint = push.IsSendToDevelopmentServer
                ? ConfigReader.Get(Constants.Environment.AppleApiDevEndpoint)
                : ConfigReader.Get(Constants.Environment.AppleApiProdEndpoint);

            string port = push.IsSendToDevelopmentServer
                ? ConfigReader.Get(Constants.Environment.AppleApiDevPort)
                : ConfigReader.Get(Constants.Environment.AppleApiProdPort);

            return $"{endpoint}:{port}/3/device/{push.Token}";
        }

        private HttpRequestMessage CreateHttpRequestMessage(ApplePush push, object payload, string url)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Version = new Version(2, 0)
            };

            req.Headers.Add("apns-priority", push.Priority.ToString());
            req.Headers.Add("apns-push-type", push.Type.ToString().ToLowerInvariant());
            req.Headers.Add("apns-topic", GetTopic(push.Type));

            if (!_useCert)            
                req.Headers.Authorization = new AuthenticationHeaderValue("bearer", GetOrGenerateJwt());    

            req.Content = new StringContent(Json.Serialize(payload, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.None
            }),
            Encoding.UTF8,
            Constants.Application.ContenTypeapplicationjson);

            return req;
        }

        public string GetTopic(ApplePushType pushType)
        {
            switch (pushType)
            {
                case ApplePushType.Background:
                case ApplePushType.Alert:
                    return _bundleId;
                case ApplePushType.Voip:
                    return _bundleId + ".voip";
                case ApplePushType.Location:
                    return _bundleId + ".location-query";
                case ApplePushType.LiveActivity:
                    return _bundleId + ".push-type.liveactivity";
                case ApplePushType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(nameof(pushType), pushType, null);
            }
        }

        string GetOrGenerateJwt()
        {
            if (_lastJwtGenerationTime > DateTime.UtcNow - TimeSpan.FromMinutes(20))
                return _jwt;

            var now = DateTimeOffset.UtcNow;

            string header = JsonConvert.SerializeObject((new { alg = "ES256", kid = _keyId }));
            string payload = JsonConvert.SerializeObject(new { iss = _teamId, iat = now.ToUnixTimeSeconds() });

            string headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
            string payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            string unsignedJwtData = $"{headerBase64}.{payloadBase64}";

            byte[] signature;

            using (var dsa = new ECDsaCng(_key))
            {
                dsa.HashAlgorithm = CngAlgorithm.Sha256;
                signature = dsa.SignData(Encoding.UTF8.GetBytes(unsignedJwtData));
            }

            _jwt = $"{unsignedJwtData}.{Convert.ToBase64String(signature)}";
            _lastJwtGenerationTime = now.UtcDateTime;

            return _jwt;

        }
    }
}
