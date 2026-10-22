using Newtonsoft.Json;

namespace VAA.EAPI.Infrastructure.Email.Entities
{
    public class MessageData
    {
        [JsonProperty("Member_First_Name")]
        public string Member_First_Name { get; set; }
        [JsonProperty("Mileage_Expiry_Date")]
        public string Mileage_Expiry_Date { get; set; }
        [JsonProperty("Mileage_Balance_As_Of_Date")]
        public string Mileage_Balance_As_Of_Date { get; set; }
        [JsonProperty("Mileage_Balance")]
        public string Mileage_Balance { get; set; }
        [JsonProperty("Membership_Number")]
        public string Membership_Number { get; set; }
        [JsonProperty("Territory")]
        public string Territory { get; set; }
        [JsonProperty("Tier")]
        public string Tier { get; set; }
        [JsonProperty("Virgin_Points")]
        public string Virgin_Points { get; set; }
        [JsonProperty("Member_Name")]
        public string Member_Name { get; set; }
        [JsonProperty("Child_First_Name")]
        public string Child_First_Name { get; set; }
        [JsonProperty("Child_Membership_Number")]
        public string Child_Membership_Number { get; set; }

    }
}
