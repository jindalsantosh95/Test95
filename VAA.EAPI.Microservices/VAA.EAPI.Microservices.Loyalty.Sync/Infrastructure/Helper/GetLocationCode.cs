using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Helper
{
    public class GetLocationCode
    {
        public static string GetEmailAddressLocationCode(string emailType)
        {
            return emailType == EmailType.Personal.ToString()
                ? "H"
                : emailType == EmailType.Business.ToString() ? "B" : "O";
        }
        public static string PhoneLocationCode(string phoneType)
        {
            return phoneType == PhoneType.Landline.ToString()
                ? "H"
                : phoneType == PhoneType.Mobile.ToString() ? "B" : "O";
        }
        public static string AddressLocationCode(string addressType)
        {
            return addressType == AddressType.Work.ToString() || addressType == AddressType.Business.ToString()
                ? "B"
                : addressType == AddressType.Home.ToString() ? "H" : "O";
        }
    }
}
