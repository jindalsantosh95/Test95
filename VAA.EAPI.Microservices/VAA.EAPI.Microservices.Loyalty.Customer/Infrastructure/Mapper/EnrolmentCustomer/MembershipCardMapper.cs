using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class MembershipCardMapper
    {
        public static MembershipCard MapEAPIMembershipCardToSCVCard(CreateCustomerEntities createCustomerEntities)
        {
            return new MembershipCard()
            {
                Active = "Y",
                CardExpiryDate = string.Empty,
                CardNumber = createCustomerEntities.FcNumber,
                CardReplacementReason = string.Empty,
                CardStatusCode = StatusEnum.Active.ToString(),
                CardType = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.Basic ? TierCode.Glass.ToString() : createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount ? TierCode.VAC.ToString() : TierCode.Red.ToString(),
                Description = string.Empty,
                ExpiryDate = string.Empty,
                IssueDate = DateTime.UtcNow.Date.ToString(Constants.Application.ISODateTimeFormat),
                MemberNumber = createCustomerEntities.FcNumber,
                NameOnCard = createCustomerEntities.FirstName,
                RequestedDate = DateTime.UtcNow.Date.ToString(Constants.Application.ISODateTimeFormat),
                SourceIdentifier = string.Empty,
                SourceSystem = string.Empty,
                StartDate = DateTime.UtcNow.Date.ToString(Constants.Application.ISODateTimeFormat),
                TierCode = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.Basic ? TierCode.Glass.ToString() : createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount ? TierCode.VAC.ToString() : TierCode.Red.ToString(),
                CreatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                CardId = Utility.GetId()
            };
        }
    }
}
