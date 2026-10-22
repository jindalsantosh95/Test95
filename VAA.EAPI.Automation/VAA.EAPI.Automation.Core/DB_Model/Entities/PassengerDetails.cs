using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class PassengerDetails
    {
        [JsonProperty("SourceIdentifier")]
        public string SourceIdentifier { get; set; }

        [JsonProperty("SourceSystem")]
        public string SourceSystem { get; set; }

        [JsonProperty("CreatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("PassengerName")]
        public string PassengerName { get; set; }

        [JsonProperty("PassengerNumber")]
        public string PassengerNumber { get; set; }

        [JsonProperty("CertificateNumber")]
        public string CertificateNumber { get; set; }

        [JsonProperty("ValueRedeemed")]
        public string ValueRedeemed { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("PartnerLocation")]
        public string PartnerLocation { get; set; }

        [JsonProperty("VAReferenceAgreementNumber")]
        public string VAReferenceAgreementNumber { get; set; }

        [JsonProperty("NumberOfSegments")]
        public string NumberOfSegments { get; set; }

        [JsonProperty("PassengerTypeCode")]
        public string PassengerTypeCode { get; set; }

        [JsonProperty("PhysicalTicketNumber")]
        public string PhysicalTicketNumber { get; set; }

        [JsonProperty("TicketType")]
        public string TicketType { get; set; }

        [JsonProperty("ValueToCredit")]
        public string ValueToCredit { get; set; }

        [JsonProperty("NumberOfTickets")]
        public string NumberOfTickets { get; set; }

        [JsonProperty("TicketDocumentNumber")]
        public string TicketDocumentNumber { get; set; }

        [JsonProperty("TotalAmountSpent")]
        public string TotalAmountSpent { get; set; }

        [JsonProperty("PassengerId")]
        public string PassengerId { get; set; }

        [JsonProperty("SegmentDetails")]
        public List<SegmentDetails> SegmentDetails { get; set; }
    }
}
