
namespace VAA.EAPI.Automation.Core.DB_Model.Entities
{    
    public class MemberCustomAttributes
    {
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string ActiveYearCounterForTenureBenefits { get; set; }
        public string TotalActiveYear { get; set; }
        public MemberLastFiveVSActivityYear[] MemberLastFiveVSActivityYear { get; set; }
        public string id { get; set; }
        public string PartitionKey { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }

    public class MemberLastFiveVSActivityYear
    {
        public string ActivityYear { get; set; }
        public string BenefitGiven { get; set; }
    }

}
