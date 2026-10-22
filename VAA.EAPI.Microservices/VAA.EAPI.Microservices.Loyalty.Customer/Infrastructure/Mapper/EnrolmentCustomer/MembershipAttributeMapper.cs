using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public class MembershipAttributeMapper
    {
        public static MembershipAttribute[] MapEAPIMembershipAttributeToSCV(CreateCustomerEntities createCustomerEntities)
        {
            return new MembershipAttribute[]
            {
                new ()
                {
                    SourceIdentifier = string.Empty,
                    SourceSystem =string.Empty,
                    CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    LastUpdatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    AttributeId = string.Empty,
                    AttributeName = string.Empty,
                    Value = string.Empty
                }
            };
        }
    }
}
