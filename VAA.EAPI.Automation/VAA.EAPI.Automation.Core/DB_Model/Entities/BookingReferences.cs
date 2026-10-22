using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class BookingReferences
    {
        [JsonProperty("RLOC")]
        public string RLOC { get; set; }

    }
}
