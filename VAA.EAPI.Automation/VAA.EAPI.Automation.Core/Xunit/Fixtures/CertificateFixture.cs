using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace VAA.EAPI.Automation.Core.Xunit.Fixtures
{
    public sealed class CertificateFixture : IDisposable
    {
        private static readonly ConcurrentDictionary<string, X509Certificate2> cache = new();

        public X509Certificate2 GetCertificate(string key)
        {
            return string.IsNullOrWhiteSpace(key)
                ? throw new ArgumentNullException(key, nameof(key))
                : !cache.ContainsKey(key) ? null : cache[key];
        }

        public X509Certificate2 LoadCertificate(string fileName, string password = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(fileName, nameof(fileName));
            if (cache.ContainsKey(fileName))
                return cache[fileName];

            FileInfo fileInfo = new (fileName);

            if (!fileInfo.Exists)
                throw new FileNotFoundException();

            while (!cache.ContainsKey(fileInfo.FullName) && !cache.TryAdd(fileInfo.FullName,
                X509Certificate2.GetCertContentType(fileName) == X509ContentType.Pkcs12
                    ? X509CertificateLoader.LoadPkcs12FromFile(fileName, password)
                    : X509CertificateLoader.LoadCertificateFromFile(fileName)))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return cache[fileName];
        }

        public X509Certificate2 ImportCertificate(string key, Vault vault, string certificateSecret, string certificatePasswordSecret = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(key, nameof(key));
            if (cache.ContainsKey(key))
                return cache[key];
            if (vault == null)
                ArgumentNullException.ThrowIfNull(vault, nameof(vault));
            if (string.IsNullOrWhiteSpace(certificateSecret))
                throw new ArgumentNullException(certificateSecret, nameof(certificateSecret));

            string data = vault.GetSecretValue(certificateSecret);
            string password = null;

            if (!string.IsNullOrWhiteSpace(certificatePasswordSecret))
                password = vault.GetSecretValue(certificatePasswordSecret);

            return ImportCertificate(key, data, password);
        }
        public X509Certificate2 ImportCertificate(string key, string data, string password = null) => ImportCertificate(key, Convert.FromBase64String(data), password);
        public X509Certificate2 ImportCertificate(string key, byte[] data, string password = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(key, nameof(key));
            if (cache.ContainsKey(key))
                return cache[key];
            if (data == null || !data.Any())
                ArgumentNullException.ThrowIfNull(data, nameof(data));

            while (!cache.ContainsKey(key) && !cache.TryAdd(key,
                X509Certificate2.GetCertContentType(data) == X509ContentType.Pkcs12
                    ? X509CertificateLoader.LoadPkcs12(data, password)
                    : X509CertificateLoader.LoadCertificate(data)))
                if (!Thread.Yield())
                    Thread.Sleep(0);

            return cache[key];
        }

        public void Dispose()
        {
        }
    }
}
