using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities
{
    public class CustomerAttribute
    {
        public enum Types
        {
            [EnumMember(Value = "IphoneApp")]
            IphoneApp = 0,
            [EnumMember(Value = "TwitterHandle")]
            TwitterHandle = 1,
            [EnumMember(Value = "VAFacebookApp")]
            VAFacebookApp = 2
        }

        public Types? Type { get; set; }
        public string Value { get; set; }
    }
}