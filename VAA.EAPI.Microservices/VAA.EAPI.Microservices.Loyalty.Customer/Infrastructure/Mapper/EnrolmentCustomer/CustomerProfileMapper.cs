using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class CustomerProfileMapper
    {
        public static CustomerProfile MapEAPICustomerProfileToSCVCustomerProfile(CreateCustomerEntities createCustomerEntities, string docType)
        {
            return new CustomerProfile()
            {
                CustomerId = createCustomerEntities.CustomerId,
                Preferences = createCustomerEntities?.Preferences == null ? new Preference[0] : PreferenceMapper.MapEAPIPreferenceToSCVPreference(createCustomerEntities),
                DocType = docType,
                Id = createCustomerEntities?.CustomerId,
                PartitionKey = createCustomerEntities?.CustomerId,
                ReasonForTravel = createCustomerEntities?.ReasonForTravel,
                Brand = Constants.Application.Brand,
                CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities?.EnrollmentChannel,
                LastUpdatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities?.EnrollmentChannel,
                LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                SourceSystem = string.Empty,
                SourceIdentifier = string.Empty,
                SchemaVersion = Constants.Application.SchemaVersion
            };
        }
    }
}
