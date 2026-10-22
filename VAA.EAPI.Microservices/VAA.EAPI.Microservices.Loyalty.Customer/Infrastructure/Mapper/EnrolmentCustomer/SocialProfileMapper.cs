using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class SocialProfileMapper
    {
        public static SocialProfile[] MapEAPISocialProfileToSCVSocialProfile(CreateCustomerEntities createCustomerEntities)
        {
            return MapSocialProfile(createCustomerEntities);
        }

        public static SocialProfile[] MapSocialProfile(CreateCustomerEntities createCustomerEntities)
        {
            List<SocialProfile> profileGrp = new List<SocialProfile>();
            createCustomerEntities?.CustomerAttribute?.ForEach(ca =>
            {
                SocialProfile socialProfile = new SocialProfile()
                {
                    LastUpdatedBy = string.Empty,
                    Profile = ca?.Type?.ToString(),
                    ProfileValue = ca?.Value?.ToString(),
                    CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    CreatedOn = DateTime.Now.ToString("yyyy-MM-dd"),
                    LastUpdatedOn = string.Empty,
                    ProfileId = string.Empty,
                    SourceSystem = string.Empty,
                    SourceIdentifier = string.Empty
                };
                profileGrp.Add(socialProfile);
            });
            return profileGrp.ToArray();
        }

    }
}
