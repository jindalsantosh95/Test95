using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Text;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;


namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service
{
    public class PgpEncryptionService : IPgpEncryptionService
    {
        public string Encrypt(string json, string publicKeyContent)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("JSON payload cannot be empty.", nameof(json));
            }

            if (string.IsNullOrWhiteSpace(publicKeyContent))
            {
                throw new ArgumentException("PGP public key cannot be empty.", nameof(publicKeyContent));
            }

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(json);

            using var outputStream = new MemoryStream();

            using Stream publicKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(publicKeyContent));

            PgpPublicKey publicKey = ReadEncryptionPublicKey(publicKeyStream);

            var encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, true, new Org.BouncyCastle.Security.SecureRandom());

            encryptedDataGenerator.AddMethod(publicKey);

            using (Stream encryptedStream = encryptedDataGenerator.Open(outputStream, new byte[1 << 16]))
            {
                var compressedDataGenerator = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);

                using (Stream compressedStream = compressedDataGenerator.Open(encryptedStream))
                {
                    var literalDataGenerator = new PgpLiteralDataGenerator();

                    using (Stream literalStream = literalDataGenerator.Open(compressedStream, PgpLiteralData.Binary, "payload.json", plainTextBytes.Length, DateTime.UtcNow)) { literalStream.Write(plainTextBytes, 0, plainTextBytes.Length); }
                }
            }

            return Convert.ToBase64String(outputStream.ToArray());
        }

        private static PgpPublicKey ReadEncryptionPublicKey(Stream publicKeyStream)
        {
            var keyRingBundle = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(publicKeyStream));

            foreach (PgpPublicKeyRing keyRing in keyRingBundle.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys()) { if (key.IsEncryptionKey) { return key; } }
            }

            throw new InvalidOperationException("No encryption-capable PGP public key was found.");
        }
    }
}