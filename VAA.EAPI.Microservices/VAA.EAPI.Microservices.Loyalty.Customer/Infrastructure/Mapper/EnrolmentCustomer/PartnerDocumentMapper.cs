using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class PartnerDocumentMapper
    {
        public static PartnerMembership MapPartnerDocument(CreateCustomerEntities createCustomerEntities, string docType)
        {
            return new PartnerMembership()
            {
                CustomerId = createCustomerEntities?.CustomerId,
                StartDate = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                PartnerStatusCode = createCustomerEntities?.PartnerStatusCode,
                PartnerMembershipMappingId = createCustomerEntities?.PartnerMembershipMappingId,
                CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                DocType = docType,
                PartitionKey = createCustomerEntities?.CustomerId,
                Id = Utility.GetId(),
                Brand = Constants.Application.Brand,
                SchemaVersion = Constants.Application.SchemaVersion,
                SourceIdentifier = string.Empty,
                SourceSystem = string.Empty,
                CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                LastUpdatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                Description = string.Empty,
                PartnerId = string.Empty,
                ProgramId = string.Empty,
                EndDate = string.Empty,
                PartnerMemberNumber = string.Empty,
                MemberNumber = createCustomerEntities?.FcNumber,
            };
        }
    }
}

