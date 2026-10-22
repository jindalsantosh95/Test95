using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink;

namespace VAA.EAPI.Microservices.Retail.Trip.Helper
{
    public static class DeeplinkTokenHelper
    {
        public static async Task<string> GenerateTokenForPOPSLink(DeepLinkRequestPayload request)
        {
            var recordLocator = BookingIdHelper.GetPnrLocator(request?.TripReservationId).GetAwaiter().GetResult();
            var flightTemp = request?.FlightNumber?.PadLeft(6, '0'); 
            var keyVaultvalue = ConfigReader.Try<string>(Constant.Environment.POPSTokenKey);

            try
            {
                byte[] clearBytes = Encoding.Unicode.GetBytes(recordLocator);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(keyVaultvalue, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        string temp = Convert.ToBase64String(ms.ToArray());

                        //Append flight number
                        string temp1 = temp + flightTemp;

                        //encode the encrypted PNR
                        var plainTextBytes = Encoding.UTF8.GetBytes(temp1);
                        return Convert.ToBase64String(plainTextBytes);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
