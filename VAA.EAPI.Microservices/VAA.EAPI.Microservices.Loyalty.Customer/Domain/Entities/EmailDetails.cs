using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
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
        public string Valid { get; set; }
    }
}
