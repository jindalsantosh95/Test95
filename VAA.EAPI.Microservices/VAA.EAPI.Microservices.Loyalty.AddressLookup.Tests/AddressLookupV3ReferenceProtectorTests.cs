using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3;
using Xunit;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public sealed class AddressLookupV3ReferenceProtectorTests
    {
        public AddressLookupV3ReferenceProtectorTests()
        {
            ConfigReader.Set(Constants.Environment.AddressLookupV3ReferenceEncryptionKey, Convert.ToBase64String(new byte[32]
            {
                1, 2, 3, 4, 5, 6, 7, 8,
                9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24,
                25, 26, 27, 28, 29, 30, 31, 32
            }));
        }

        [Fact]
        public void Unprotect_WhenReferenceIsTampered_RejectsItAsUnprocessable()
        {
            var protector = new AddressLookupV3ReferenceProtector();
            var protectedReference = protector.Protect(new AddressLookupV3Reference
            {
                ContractVersion = "v2",
                ProviderId = "provider-address-id",
                ItemType = "Address",
                FlowId = "flow-id",
                SelectionIntentId = "selection-intent-id",
                CountryCode = "GB",
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(1)
            });

            var parts = protectedReference.Split('.');
            parts[3] = (parts[3][0] == 'A' ? "B" : "A") + parts[3][1..];
            var tamperedReference = string.Join('.', parts);

            var exception = Assert.Throws<AddressLookupV3Exception>(() => protector.Unprotect(tamperedReference));

            Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
            Assert.Equal("InvalidReference", exception.Code);
        }

        [Fact]
        public void Unprotect_AcceptsPreviouslyIssuedPascalCasePayload()
        {
            // This is the wire shape emitted before switching to the shared serializer.
            var plaintext = Encoding.UTF8.GetBytes("{\"ContractVersion\":\"v1\",\"ProviderId\":\"test-address\",\"ItemType\":\"Address\",\"FlowId\":\"test-flow\",\"SelectionIntentId\":\"test-intent\",\"CountryCode\":\"GB\",\"CallerKeyClass\":\"account\",\"Dataset\":\"GB\",\"ExpiresAtUtc\":\"2026-09-29T12:00:00+00:00\"}");
            var nonce = RandomNumberGenerator.GetBytes(12);
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[16];
            using var cipher = new AesGcm(Convert.FromBase64String(ConfigReader.Get(Constants.Environment.AddressLookupV3ReferenceEncryptionKey)), 16);
            cipher.Encrypt(nonce, plaintext, ciphertext, tag, Encoding.UTF8.GetBytes("address-lookup-v3-reference"));
            var encoded = string.Join('.', "v1", Base64UrlEncoder.Encode(nonce), Base64UrlEncoder.Encode(tag), Base64UrlEncoder.Encode(ciphertext));

            var result = new AddressLookupV3ReferenceProtector().Unprotect(encoded);

            Assert.Equal("test-address", result.ProviderId);
            Assert.Equal("test-intent", result.SelectionIntentId);
            Assert.Equal(new DateTimeOffset(2026, 9, 29, 12, 0, 0, TimeSpan.Zero), result.ExpiresAtUtc);
            var roundTrip = new AddressLookupV3ReferenceProtector().Unprotect(new AddressLookupV3ReferenceProtector().Protect(result));
            Assert.Equal(result.ProviderId, roundTrip.ProviderId);
            Assert.Equal(result.ExpiresAtUtc, roundTrip.ExpiresAtUtc);
        }

        [Fact]
        public void Unprotect_WhenNonceOrTagHasInvalidLength_RejectsItAsUnprocessable()
        {
            var protector = new AddressLookupV3ReferenceProtector();

            var exception = Assert.Throws<AddressLookupV3Exception>(() => protector.Unprotect("v1.AA.AA.AA"));

            Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
            Assert.Equal("InvalidReference", exception.Code);
        }

    }
}
