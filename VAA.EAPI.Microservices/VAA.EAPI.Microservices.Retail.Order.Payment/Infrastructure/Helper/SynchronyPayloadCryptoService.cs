using Jose;
using System;
using System.Security.Cryptography;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper
{
    public class SynchronyPayloadCryptoService : ISynchronyPayloadCryptoService
    {
        public string EncryptData(string data)
        {
            var publicKey = ConfigReader.Try<string>(Constants.Environment.SynchronyEncryptionPublicKey);
            var keyParams = Json.Deserialize<JweRsaKey>(publicKey);

            RSAParameters rsaParams = new RSAParameters
            {
                Modulus = Base64UrlDecode(keyParams.N),
                Exponent = Base64UrlDecode(keyParams.E)
            };

            using var rsa = RSA.Create();
            rsa.ImportParameters(rsaParams);

            return JWT.Encode(data, rsa, JweAlgorithm.RSA_OAEP_256, JweEncryption.A128CBC_HS256);

        }

        private static byte[] Base64UrlDecode(string input)
        {
            string padded = input.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return Convert.FromBase64String(padded);
        }
    }
}
