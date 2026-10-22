using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.Apns;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Helper
{
    public class PushNotificationService
    {
        public static string GetCertificate() => KeyVault.Default.GetSecretValue(ConfigReader.Get(Constants.Environment.APNSPushCertificate));

        public static async Task<bool> PushNotification(IHttpConnector httpConnector, string token)
        {

            var options = new ApnsJwtOptions()
            {
                BundleId = ConfigReader.Get(Constants.Environment.PassTypeIdentifier),
                CertContent = CertificateContent(GetCertificate()),
                KeyId = ConfigReader.Get(Constants.Environment.PushNotificationKeyId),
                TeamId = ConfigReader.Get(Constants.Environment.TeamIdentifier)
            };

            var push = new ApplePush(ApplePushType.Alert)
                            .AddAlert(Constants.Application.Title, Constants.Application.Body)
                            .AddToken(token)
                            .SendToDevelopmentServer(ConfigReader.Get<bool>(Constants.Environment.IsDevelopmentServer));


            var response = await ApnsClient.CreateUsingJwt(httpConnector, options).SendAsync(push);

            if (!response.IsSuccessful)
            {
                throw new Exception(response.ReasonString);
            }

            return true;

        }
        private static CngKey CertificateContent(string certContent)
        {
            certContent = certContent.Replace("\\r", "\r").Replace("\\n", "\n");
            certContent = certContent.Replace("\r\n", "")
                                     .Replace("-----BEGIN PRIVATE KEY-----", "")
                                     .Replace("-----END PRIVATE KEY-----", "");

            return CngKey.Import(Convert.FromBase64String(certContent), CngKeyBlobFormat.Pkcs8PrivateBlob);
        }
    }
}
