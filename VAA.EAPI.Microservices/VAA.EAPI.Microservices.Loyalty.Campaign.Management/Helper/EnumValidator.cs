using System;
using VAA.EAPI.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Helper
{
    public class EnumValidator
    {
        public static string GetEnumValueByDescription<T>(string description) where T : Enum
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            foreach (Enum enumItem in Enum.GetValues(typeof(T)))
            {
                if (EnumOperations.GetEnumDescription(enumItem) == description)
                {
                    return enumItem.ToString();
                }
            }
            return description;
        }
    }
}
