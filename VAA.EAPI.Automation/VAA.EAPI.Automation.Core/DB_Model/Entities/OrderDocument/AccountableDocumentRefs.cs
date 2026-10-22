using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument
{
    public class AccountableDocumentRefs
    {
        [JsonProperty("DocumentType")]
        public string DocumentType { get; set; }

        [JsonProperty("DocumentNumber")]
        public string DocumentNumber { get; set; }

        [JsonProperty("IssueDate")]
        public string IssueDate { get; set; }

        [JsonProperty("CustomerRefId")]
        public string CustomerRefId { get; set; }

        [JsonProperty("AllFlown")]
        public bool AllFlown { get; set; }

        [JsonProperty("FirstNameNumber")]
        public string FirstNameNumber { get; set; }

        [JsonProperty("LastNameNumber")]
        public string LastNameNumber { get; set; }
    }
}