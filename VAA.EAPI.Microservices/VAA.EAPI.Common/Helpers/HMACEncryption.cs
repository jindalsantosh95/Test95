using System;
using System.Security.Cryptography;
using System.Text;

namespace VAA.EAPI.Common.Helpers
{
    public static class HMACEncryption
    {
        public static string Encrypt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA512.HashData(plainTextBytes);

            return (Convert.ToHexString(hashBytes)).ToLower();
        }

    }
}


