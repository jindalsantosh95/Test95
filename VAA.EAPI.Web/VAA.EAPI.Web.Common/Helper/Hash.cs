using System.Security.Cryptography;
using System.Text;

namespace VAA.EAPI.Web.Common.Helper
{
    public static class Hash
    {
        public static readonly int _saltLength = 16;
        public static string Compute(string input)
        {
            byte[] salt;
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            new RNGCryptoServiceProvider().GetBytes(salt = new byte[_saltLength]);

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(input);
            byte[] plainTextByteWithSalt = new byte[plainTextBytes.Length + salt.Length];

            Array.Copy(plainTextBytes, 0, plainTextByteWithSalt, 0, plainTextBytes.Length);
            Array.Copy(salt, 0, plainTextByteWithSalt, plainTextBytes.Length, _saltLength);

            SHA256Managed hash = new();
            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextByteWithSalt);

            byte[] hashBytesWithSalt = new byte[hashBytes.Length + salt.Length];
            Array.Copy(salt, 0, hashBytesWithSalt, 0, _saltLength);
            Array.Copy(hashBytes, 0, hashBytesWithSalt, _saltLength, hashBytes.Length);

            string savedPasswordHash = Convert.ToBase64String(hashBytesWithSalt);
            return savedPasswordHash;
        }

        public static string SHA256Managed(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(input);
            SHA256Managed hash = new();
            byte[] hashBytes = hash.ComputeHash(plainTextBytes);
            return Convert.ToBase64String(hashBytes);
        }

    }
}
