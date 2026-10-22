using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class PreferenceMapper
    {
        public static Preference[] MapEAPIPreferenceToSCVPreference(CreateCustomerEntities createCustomerEntities)
        {
            return MapPreferences(createCustomerEntities);
        }

        public static Preference[] MapPreferences(CreateCustomerEntities createCustomerEntities)
        {
            List<Preference> preferenceGrp = new List<Preference>();

            createCustomerEntities?.Preferences?.ForEach(p =>
            {
                Preference preferences = new Preference()
                {
                    Persona = "Default",
                    PreferenceSubType = string.Empty,
                    PreferenceType = p?.PreferenceType.ToString(),
                    Active = createCustomerEntities.Status == StatusEnum.Active.ToString() ? "Y" : "N",
                    CaptureCannel = string.Empty,
                    PreferenceId = Utility.GetId(),
                    PreferenceValue = p?.PreferenceValue,
                    SourceSystem = string.Empty,
                    SourceIdentifier = string.Empty,
                    LastUpdatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities?.EnrollmentChannel,
                    CommsType = Array.Empty<CommsType>(),
                    CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities?.EnrollmentChannel,
                    CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat)
                };
                preferenceGrp.Add(preferences);
            });
            return preferenceGrp.ToArray();
        }
    }
}
