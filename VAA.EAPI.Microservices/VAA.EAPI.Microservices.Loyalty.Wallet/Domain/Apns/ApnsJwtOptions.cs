using System;
using System.Security.Cryptography;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.Apns
{
    public class ApnsJwtOptions
    {
        public string KeyId { get; set; }

        public CngKey CertContent { get; set; }

        public string TeamId { get; set; }

        public string BundleId { get; set; }


    }
}
