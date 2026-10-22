using Newtonsoft.Json;


namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class ContactNote
    {

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("Brand")]
        public string Brand { get; set; }

        [JsonProperty("DocType")]
        public string DocType { get; set; }

        [JsonProperty("PartitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("NoteTypeGroup")]
        public string NoteTypeGroup { get; set; }

        [JsonProperty("NoteType")]
        public string NoteType { get; set; }

        [JsonProperty("NoteId")]
        public string NoteId { get; set; }

    }
}
