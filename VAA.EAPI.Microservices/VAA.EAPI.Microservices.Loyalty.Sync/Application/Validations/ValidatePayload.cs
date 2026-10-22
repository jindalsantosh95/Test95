using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Validations
{
    public class ValidatePayload
    {
        public static void Validate(LoyaltySyncRequestData loyaltySyncRequestData)
        {
            string addressLine1 = loyaltySyncRequestData.AddressDetails.First(x => x.PreferredFlag == "Y").AddressLine1;
            string addressLine2 = loyaltySyncRequestData.AddressDetails.First(x => x.PreferredFlag == "Y").AddressLine2;
            string city = loyaltySyncRequestData.AddressDetails.First(x => x.PreferredFlag == "Y").City;

            loyaltySyncRequestData.FirstName = RemoveSpecialCharInName(loyaltySyncRequestData.FirstName, 25);
            loyaltySyncRequestData.LastName = RemoveSpecialCharInName(loyaltySyncRequestData.LastName, 40);
            loyaltySyncRequestData.MiddleName=loyaltySyncRequestData.MiddleName != null ? RemoveSpecialCharAndTrim(loyaltySyncRequestData.MiddleName, 25) : null;
            loyaltySyncRequestData.AddressDetails.First(x => x.PreferredFlag == "Y").AddressLine1 = addressLine1 != null ? RemoveSpecialCharAndTrim(addressLine1, 40) : addressLine1;
            loyaltySyncRequestData.AddressDetails.First(x => x.PreferredFlag == "Y").AddressLine2 = addressLine2 != null ? RemoveSpecialCharAndTrim(addressLine2, 40) : addressLine2;
            loyaltySyncRequestData.AddressDetails.First(x => x.PreferredFlag == "Y").City = city != null ? RemoveSpecialCharAndTrim(city, 40) : city;
        }
        
        private static string RemoveSpecialCharInName(string name, int length)
        {
            name = RegExHelper.RemoveSpecialCharacter(name);

            if (name.Length > length)
                return name.Substring(0, length);

            return name;
        }

        private static string RemoveSpecialCharAndTrim(string name, int length)
        {
            name = RegExHelper.RemoveSpecialChar(name);

            if (name.Length > length)
                return name.Substring(0, length);

            return name;
        }
    }
}
