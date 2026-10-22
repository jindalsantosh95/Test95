using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class CreditdetailDTO
    {
        public string CreditItemId { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
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

        public CreditdetailDTO()
        {

        }

        public static explicit operator CreditdetailDTO(Creditdetail creditdetail)
        {
            return new()
            {
                CreditItemId = string.IsNullOrEmpty(creditdetail?.CreditItemId) ? Utility.GetId() : creditdetail?.CreditItemId,
                CreatedOn = string.IsNullOrEmpty(creditdetail?.CreditItemId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : creditdetail?.CreatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = creditdetail?.CreatedBy,
                LastUpdatedOn = string.IsNullOrEmpty(creditdetail?.CreditItemId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : creditdetail?.LastUpdatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = creditdetail?.LastUpdatedBy,
                CreditValue = !string.IsNullOrEmpty(creditdetail?.CreditValue) ? ((int)Convert.ToDouble(creditdetail?.CreditValue)).ToString() : string.Empty,
                PromotionRuleActionId = creditdetail?.PromotionRuleActionId,
                PointType = creditdetail?.PointType,
                Comments = creditdetail?.Comments,
                PromotionId = creditdetail?.PromotionId,
                PointSubType = creditdetail?.PointSubType,
                QualifyingFlag = creditdetail?.QualifyingFlag,
                CreditValueUsed = creditdetail?.CreditValueUsed,
                SourceIdentifier = creditdetail?.SourceIdentifier,
                SourceSystem = creditdetail?.SourceSystem
            };
        }
    }
}
