using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class PersonaMapper
    {
        public static List<Persona> MapEAPIPersonaToSCVPersona(CreateCustomerEntities createCustomerEntities)
        {
            string UpdatedLastName = GetLastName.PopulateLastName(createCustomerEntities);
            return new List<Persona>()
            {
                new ()
                {
                   PreferredGender = createCustomerEntities?.PreferredGender,
                   FirstName = createCustomerEntities?.FirstName,
                   MiddleName = createCustomerEntities?.MiddleName,
                   IsPersonaVisible = string.Empty,
                   PreferredPronouns = createCustomerEntities?.PreferredPronouns,
                   PreferredName = createCustomerEntities?.PreferredName,
                   LastName = UpdatedLastName,
                   CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                   PreferredSalutation = createCustomerEntities?.PreferredSalutation,
                   LastUpdatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                   CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                   DBLastUpdatedSrc = string.Empty,
                   DBLastUpdatedOn = string.Empty,
                   LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat)
                }
            };
        }
    }
}
