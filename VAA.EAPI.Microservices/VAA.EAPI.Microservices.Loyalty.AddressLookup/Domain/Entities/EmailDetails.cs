using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities
{
    public class EmailDetails
    {
        public enum Usages
        {
            [EnumMember(Value = "Business")]
            Business = 0,
            [EnumMember(Value = "Personal")]
            Personal = 1
        }

        public Usages? Usage { get; set; }
        public string EmailId { get; set; }
        public string PreferredFlag { get; set; }
        public string Description { get; set; }
    }
}