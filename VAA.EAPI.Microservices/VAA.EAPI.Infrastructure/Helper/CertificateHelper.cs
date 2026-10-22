using System;
using System.Security.Cryptography.X509Certificates;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Helper
{
    public static class CertificateHelper
    {
        public static X509Certificate2 GetCertificate(string secret) => new(Convert.FromBase64String(Services.KeyVault.Default.GetSecretValue(ConfigReader.Get(secret))), string.Empty, X509KeyStorageFlags.MachineKeySet);

        public static X509Certificate2 GetCachedCertificate()
        {
            if (Constants.Constants.DlCertificate is not null) return Constants.Constants.DlCertificate;

            Constants.Constants.DlCertificate = GetCertificate(Constants.Constants.DeltaCertificateName);

            return Constants.Constants.DlCertificate;
        }
    }
}
