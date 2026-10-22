using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace VAA.EAPI.Microservices.Security.Helper
{
    public static class TokenHelper
    {
        public static string EncryptDataWithAes(string memberDetails, string keyBase64)
        {
            byte[] utfBytes = Encoding.UTF8.GetBytes(memberDetails);
            byte[] byteKey = ParseHexKey(keyBase64);

            using (Aes aesAlg = CreateAesInstance(byteKey))
            {
                ICryptoTransform encryptor = aesAlg.CreateEncryptor();
                byte[] encryptedBytes = encryptor.TransformFinalBlock(utfBytes, 0, utfBytes.Length);

                return BitConverter.ToString(encryptedBytes).Replace("-", "").ToLower();
            }
        }

        private static byte[] ParseHexKey(string hexKey)
        {
            return Enumerable.Range(0, hexKey.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hexKey.Substring(x, 2), 16))
                            .ToArray();
        }

        private static Aes CreateAesInstance(byte[] key)
        {
            Aes aesAlg = Aes.Create();
            aesAlg.Mode = CipherMode.ECB;
            aesAlg.Key = key;
            return aesAlg;
        }

    }
}
