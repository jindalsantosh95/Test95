
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Text;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.AutoAccrual.Interface;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Services
{
    public class PgpDecryptionService : IPgpDecryptionService
    {
        public string Decrypt(string encryptedPayload, string privateKey)
        {
            if (string.IsNullOrWhiteSpace(encryptedPayload))
            {
                throw new ArgumentException("Encrypted payload cannot be empty.", nameof(encryptedPayload));
            }

            if (string.IsNullOrWhiteSpace(privateKey))
            {
                throw new ArgumentException("Private key cannot be empty.", nameof(privateKey));
            }

            try
            {
                using var encryptedStream = new MemoryStream(Convert.FromBase64String(encryptedPayload));
                using var privateKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKey));

                var decoderStream = PgpUtilities.GetDecoderStream(encryptedStream);
                var pgpObjectFactory = new PgpObjectFactory(decoderStream);
                var pgpObject = pgpObjectFactory.NextPgpObject();

                if (pgpObject is not PgpEncryptedDataList encryptedDataList)
                {
                    throw new InvalidOperationException("Invalid PGP encrypted payload.");
                }

                var secretKeyRingBundle = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

                PgpPublicKeyEncryptedData? encryptedData = null;
                PgpPrivateKey? privateKeyObject = null;

                foreach (PgpPublicKeyEncryptedData encryptedDataItem in encryptedDataList.GetEncryptedDataObjects())
                {
                    var secretKey = secretKeyRingBundle.GetSecretKey(encryptedDataItem.KeyId);

                    if (secretKey == null)
                    {
                        continue;
                    }

                    privateKeyObject = secretKey.ExtractPrivateKey(null);
                    encryptedData = encryptedDataItem;
                    break;
                }

                if (encryptedData == null || privateKeyObject == null)
                {
                    throw new InvalidOperationException("No matching private key found for the encrypted PGP payload.");
                }

                using var clearStream = encryptedData.GetDataStream(privateKeyObject);
                var clearObjectFactory = new PgpObjectFactory(clearStream);
                var messageObject = clearObjectFactory.NextPgpObject();

                if (messageObject is PgpCompressedData compressedData)
                {
                    using var compressedStream = compressedData.GetDataStream();
                    var compressedObjectFactory = new PgpObjectFactory(compressedStream);
                    messageObject = compressedObjectFactory.NextPgpObject();
                }

                if (messageObject is not PgpLiteralData literalData)
                {
                    throw new InvalidOperationException("Invalid PGP literal data.");
                }

                using var literalStream = literalData.GetInputStream();
                using var outputStream = new MemoryStream();
                literalStream.CopyTo(outputStream);

                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to decrypt PGP payload.", ex);
            }
        }
    }
}
