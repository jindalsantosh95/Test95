using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class OrderDocuments
    {
        [JsonProperty("BookingReferences")]
        public BookingReferences[] BookingReferences { get; set; }

    }
}
