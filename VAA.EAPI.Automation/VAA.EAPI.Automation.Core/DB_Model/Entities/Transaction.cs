using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{
    public class Transaction
    {

        [JsonProperty("SchemaVersion")]
        public string SchemaVersion { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

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
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("CreatedBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("LastUpdatedOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdatedOn { get; set; }

        [JsonProperty("LastUpdatedBy")]
        public string LastUpdatedBy { get; set; }

        [JsonProperty("CancelledTransactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string CancelledTransactionId { get; set; }

        [JsonProperty("CancellationReasonCode")]
        public string CancellationReasonCode { get; set; }

        [JsonProperty("Comments")]
        public string Comments { get; set; }

        [JsonProperty("ClassCode")]
        public string ClassCode { get; set; }

        [JsonProperty("ActivityNumber")]
        public string ActivityNumber { get; set; }

        [JsonProperty("TransactionIntegrationId")]
        public string TransactionIntegrationId { get; set; }


        [JsonProperty("MemberNumber")]
        public string MemberNumber { get; set; }

        [JsonProperty("TransactionValue")]
        public string TransactionValue { get; set; }

        [JsonProperty("ActivityMultiplier")]
        public string ActivityMultiplier { get; set; }

        [JsonProperty("OwnerId")]
        public string OwnerId { get; set; }

        [JsonProperty("PartnerId")]
        public string PartnerId { get; set; }

        [JsonProperty("ParentTransactionId")]
        public string ParentTransactionId { get; set; }

        [JsonProperty("ProcessingComment")]
        public string ProcessingComment { get; set; }

        [JsonProperty("Processinglog")]
        public string Processinglog { get; set; }

        [JsonProperty("TransactionDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? TransactionDate { get; set; }

        [JsonProperty("ProductId")]
        public string ProductId { get; set; }

        [JsonProperty("TransactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("StatusCode")]
        public string StatusCode { get; set; }

        [JsonProperty("SubStatus")]
        public string SubStatus { get; set; }

        [JsonProperty("TransactionSubType")]
        public string TransactionSubType { get; set; }

        [JsonProperty("TransferMemberNumber")]
        public string TransferMemberNumber { get; set; }

        [JsonProperty("TransferTransactionId")]
        public string TransferTransactionId { get; set; }

        [JsonProperty("TransactionChannel")]
        public string TransactionChannel { get; set; }

        [JsonProperty("ActivityDate")]
        public DateTimeOffset? ActivityDate { get; set; }

        [JsonProperty("TransactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("AwardId")]
        public string AwardId { get; set; }

        [JsonProperty("VoucherTypeId")]
        public string VoucherTypeId { get; set; }

        [JsonProperty("CancelledBy")]
        public string CancelledBy { get; set; }

        [JsonProperty("StatementText")]
        public string StatementText { get; set; }

        [JsonProperty("ReasonCode")]
        public string ReasonCode { get; set; }

        [JsonProperty("TierPointsToCredit")]
        public string TierPointsToCredit { get; set; }

        [JsonProperty("ManuallyUpdatedBy")]
        public string ManuallyUpdatedBy { get; set; }

        [JsonProperty("PDMDocNum")]
        public string PDMDocNum { get; set; }

        [JsonProperty("PDMPartnerFileId")]
        public string PDMPartnerFileId { get; set; }

        [JsonProperty("PDMRecordId")]
        public string PDMRecordId { get; set; }

        [JsonProperty("VGLCExportFlag")]
        public string VGLCExportFlag { get; set; }

        [JsonProperty("PDMFileType")]
        public string PDMFileType { get; set; }

        [JsonProperty("NoOfTickets")]
        public string NoOfTickets { get; set; }

        [JsonProperty("FlightDetails")]
        public FlightDetails FlightDetails { get; set; }

        [JsonProperty("CreditDetails")]
        public List<CreditDetails> CreditDetails { get; set; }

        [JsonProperty("FinancialCreditDetails")]
        public FinancialCreditDetails? FinancialCreditDetails { get; set; }


    }
    public class FinancialCreditDetails
    {
        public string Amount { get; set; }
        public string PartitionName { get; set; }
        public string PANNumber { get; set; }
        public string WFNA { get; set; }
        public string BonusMiles { get; set; }
        public string GiftMiles { get; set; }
        public string NonGiftAdjustments { get; set; }
        public string StandardMiles { get; set; }
        public string AwardIndicator { get; set; }
        public string VAReward1Code { get; set; }
        public string VAReward2Code { get; set; }
        public string VAReward3Code { get; set; }
        public string VAReward4Code { get; set; }
        public string VAReward5Code { get; set; }
        public string VAReward6Code { get; set; }
        public string VAReward7Code { get; set; }
        public string SkipActivityStatement { get; set; }
    }
}
