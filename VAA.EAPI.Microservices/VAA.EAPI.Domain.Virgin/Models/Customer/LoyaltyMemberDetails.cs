using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Customer
{

    public class LoyaltyMemberDetails
    {
        public string MemberType { get; set; }
        public string ProgramName { get; set; }
        public string MemberNumber { get; set; }
        public string MemberTierCode { get; set; }
        public string Status { get; set; }
        public string ProgramCode { get; set; }
        public bool PayWithPoints { get; set; }

        public static explicit operator LoyaltyMemberDetails(JObject source)
        {
            return new LoyaltyMemberDetails()
            {
                MemberType = source["MemberType"]?.ToString(),
                ProgramName = source["ProgramName"]?.ToString(),
                MemberNumber = source["MemberNumber"]?.ToString(),
                MemberTierCode = source["MemberTierCode"]?.ToString(),
                Status = source["Status"]?.ToString(),
                ProgramCode = source["ProgramCode"]?.ToString(),
                PayWithPoints = source["PayWithPoints"] != null ? (bool)source["PayWithPoints"] : false
            };
        }
    }

}
