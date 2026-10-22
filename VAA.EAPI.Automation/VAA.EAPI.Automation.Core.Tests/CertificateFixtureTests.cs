using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests
{
    public sealed class CertificateFixtureTests
    {
        [Theory]
        [InlineData(X509ContentType.Cert, false)]
        [InlineData(X509ContentType.Cert, true)]
        [InlineData(X509ContentType.Pkcs12, false)]
        [InlineData(X509ContentType.Pkcs12, true)]
        public void LoadCertificate_PreservesCertificateAndPrivateKey(X509ContentType format, bool fromFile)
        {
            using X509Certificate2 original = CreateCertificate();
            using CertificateFixture fixture = new();
            const string password = "synthetic-test-password";
            byte[] data = original.Export(format, password);
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".cert");

            try
            {
                if (fromFile)
                {
                    File.WriteAllBytes(path, data);
                }

                using X509Certificate2 loaded = fromFile
                    ? fixture.LoadCertificate(path, password)
                    : fixture.ImportCertificate(path, data, password);

                Assert.Equal(original.Thumbprint, loaded.Thumbprint);
                Assert.Equal(format == X509ContentType.Pkcs12, loaded.HasPrivateKey);
                Assert.Same(loaded, fixture.GetCertificate(path));
            }
            finally
            {
                if (fromFile)
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void ImportCertificate_RejectsIncorrectPfxPassword()
        {
            using X509Certificate2 original = CreateCertificate();
            using CertificateFixture fixture = new();
            byte[] data = original.Export(X509ContentType.Pkcs12, "synthetic-test-password");
            string key = Guid.NewGuid().ToString("N");

            Assert.ThrowsAny<CryptographicException>(() => fixture.ImportCertificate(key, data, "incorrect-password"));
            Assert.Null(fixture.GetCertificate(key));
        }

        private static X509Certificate2 CreateCertificate()
        {
            using RSA key = RSA.Create(2048);
            CertificateRequest request = new("CN=offline-test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return request.CreateSelfSigned(
                new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2100, 1, 1, 0, 0, 0, TimeSpan.Zero));
        }
    }
}
