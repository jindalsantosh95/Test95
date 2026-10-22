using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public class MembershipChangeMapper
    {
        public static MembershipChange MapEAPIMembershipChangeToSCVMemberMembershipChange(string docType, CreateCustomerEntities createCustomerEntities)
        {
            return new MembershipChange()
            {
                SchemaVersion = Constants.Application.SchemaVersion,
                MemberNumber = createCustomerEntities?.FcNumber,
                DocType = docType,
                PartitionKey = createCustomerEntities?.CustomerId,
                Id = Utility.GetId(),
                Active = "Y",
                CustomerId = createCustomerEntities?.CustomerId,
                SourceIdentifier = string.Empty,
                SourceSystem = string.Empty,
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                AppliedRuleId = string.Empty,
                AppliedPromotionId = string.Empty,
                ApprovalDate = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                TierApprovalStatus = string.Empty,
                EndDate = DateTime.UtcNow.AddYears(1).GetLastDayOfMonth().ToString(Constants.Application.DateFormatWithDefaultTime),
                OriginalTierEndDate = string.Empty,
                TierPoints = string.Empty,
                ProcessDate = string.Empty,
                ProcessingComment = string.Empty,
                ProcessingLog = string.Empty,
                ProcessingStatus = string.Empty,
                QualifyingPeriodFlag = string.Empty,
                StartDate = DateTime.UtcNow.ToString(Constants.Application.DateFormatWithDefaultTime),
                TierCodeManuallyChangedBy = string.Empty,
                TierClass = string.Empty,
                TierCode = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.Basic ? TierCode.Glass.ToString() : createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount ? TierCode.VAC.ToString() : TierCode.Red.ToString(),
                Comments = string.Empty,
                ManualTierChangeReason = string.Empty,
                QualifyingTierPoints = 0,
                Revision = 0,
                Brand = Constants.Application.Brand,
                DowngradeProhibitReason = string.Empty,
                ChangeId = string.Empty
            };
        }
    }
}
