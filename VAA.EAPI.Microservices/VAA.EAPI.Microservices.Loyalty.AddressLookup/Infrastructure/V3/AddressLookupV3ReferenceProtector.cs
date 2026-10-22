using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3
{
    public sealed class AddressLookupV3ReferenceProtector : IAddressLookupV3ReferenceProtector
    {
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const string Version = "v1";
        private static readonly byte[] associatedData = Encoding.UTF8.GetBytes("address-lookup-v3-reference");

        public string Protect(AddressLookupV3Reference reference)
        {
            if (reference is null) throw new ArgumentNullException(nameof(reference));

            var plaintext = Encoding.UTF8.GetBytes(Json.Serialize(reference));
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[TagSize];

            using var cipher = new AesGcm(GetEncryptionKey(), TagSize);
            cipher.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

            return string.Join('.', Version, ToBase64Url(nonce), ToBase64Url(tag), ToBase64Url(ciphertext));
        }

        public AddressLookupV3Reference Unprotect(string protectedReference)
        {
            if (protectedReference.IsNullOrWhiteSpace())
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");

            var parts = protectedReference.Split('.');
            if (parts.Length != 4 || !parts[0].Is(Version))
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");

            try
            {
                var nonce = FromBase64Url(parts[1]);
                var tag = FromBase64Url(parts[2]);
                var ciphertext = FromBase64Url(parts[3]);
                var plaintext = new byte[ciphertext.Length];

                using var cipher = new AesGcm(GetEncryptionKey(), TagSize);
                cipher.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);

                return Json.Deserialize<AddressLookupV3Reference>(Encoding.UTF8.GetString(plaintext))
                    ?? throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");
            }
            catch (AddressLookupV3Exception)
            {
                throw;
            }
            catch (CryptographicException)
            {
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");
            }
            catch (FormatException)
            {
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");
            }
            catch (ArgumentException)
            {
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");
            }
            catch (JsonException)
            {
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.UnprocessableEntity, "InvalidReference", "The address reference is invalid.");
            }
        }

        private static byte[] GetEncryptionKey()
        {
            var configuredKey = ConfigReader.Get(Constants.Environment.AddressLookupV3ReferenceEncryptionKey);
            if (configuredKey.IsNullOrWhiteSpace())
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ReferenceKeyUnavailable", "Address lookup is temporarily unavailable.");

            try
            {
                var key = Convert.FromBase64String(configuredKey);
                if (key.Length != 32)
                    throw new CryptographicException();

                return key;
            }
            catch (FormatException)
            {
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ReferenceKeyUnavailable", "Address lookup is temporarily unavailable.");
            }
            catch (CryptographicException)
            {
                throw new AddressLookupV3Exception(System.Net.HttpStatusCode.ServiceUnavailable, "ReferenceKeyUnavailable", "Address lookup is temporarily unavailable.");
            }
        }

        private static string ToBase64Url(byte[] value) => Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private static byte[] FromBase64Url(string value)
        {
            var paddedValue = value.Replace('-', '+').Replace('_', '/');
            var remainder = paddedValue.Length % 4;
            if (remainder > 0)
                paddedValue = paddedValue.PadRight(paddedValue.Length + 4 - remainder, '=');

            return Convert.FromBase64String(paddedValue);
        }
    }
}
