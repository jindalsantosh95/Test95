using System;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class Creditdetail
    {
        public string CreditItemId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CreditValue { get; set; }
        public string PromotionRuleActionId { get; set; }
        public string PointType { get; set; }
        public string Comments { get; set; }
        public string PromotionId { get; set; }
        public string PointSubType { get; set; }
        public string QualifyingFlag { get; set; }
        public string CreditValueUsed { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public Creditdetail()
        {

        }
        public Creditdetail(CreditdetailDTO creditdetail)
        {
            CreditItemId = creditdetail?.CreditItemId;
            CreatedOn = Common.Helpers.DateTimeHelper.ParseDateFormat(creditdetail?.CreatedOn);
            CreatedBy = creditdetail?.CreatedBy;
            LastUpdatedOn = Common.Helpers.DateTimeHelper.ParseDateFormat(creditdetail?.LastUpdatedOn);
            LastUpdatedBy = creditdetail?.LastUpdatedBy;
            CreditValue = creditdetail?.CreditValue;
            PromotionRuleActionId = creditdetail?.PromotionRuleActionId;
            PointType = creditdetail?.PointType;
            Comments = creditdetail?.Comments;
            PromotionId = creditdetail?.PromotionId;
            PointSubType = creditdetail?.PointSubType;
            QualifyingFlag = creditdetail?.QualifyingFlag;
            CreditValueUsed = creditdetail?.CreditValueUsed;
            SourceIdentifier = creditdetail?.SourceIdentifier;
            SourceSystem = creditdetail?.SourceSystem;
        }
    }
}
