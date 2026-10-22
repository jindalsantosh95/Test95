using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class ContactDetailMapper
    {
        public static Contact[] MapEAPIContactToSCVContact(CreateCustomerEntities createCustomerEntities)
        {
            Contact[] emails = MapEmailDetails(createCustomerEntities);
            Contact[] phones = MapPhoneDetails(createCustomerEntities);
            return emails.Concat(phones).ToArray();
        }

        public static Contact[] MapEmailDetails(CreateCustomerEntities createCustomerEntities)
        {
            List<Contact> emailGrp = new List<Contact>();
            createCustomerEntities?.ContactDetails?.EmailDetails?.ForEach(email =>
            {
                Contact emails = new Contact()
                {
                    ContactId = Utility.GetId(),
                    ContactType = Convert.ToString(ContactType.Email),
                    ContactSubType = string.Empty,
                    ContactValue = email?.EmailId,
                    CountryCode = string.Empty,
                    CountryWithCode = string.Empty,
                    Description = email?.Description,
                    Primary = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities?.ContactDetails?.EmailDetails.Count == 1 && createCustomerEntities.EnrollmentChannel == Constants.Application.VREDChannelID ? "Y" : email?.PreferredFlag,
                    Usage = Convert.ToString(email?.Usage),
                    Valid = email?.Valid.IfIsNullOrWhiteSpace("Y"),
                    CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    SourceIdentifier = string.Empty,
                    SourceSystem = string.Empty
                };
                emailGrp.Add(emails);
            });
            return emailGrp.ToArray();
        }

        public static Contact[] MapPhoneDetails(CreateCustomerEntities createCustomerEntities)
        {
            List<Contact> phoneGrp = new List<Contact>();
            createCustomerEntities?.ContactDetails?.PhoneDetails?.ForEach(phone =>
            {
                Contact phones = new Contact()
                {
                    ContactId = Utility.GetId(),
                    ContactType = Convert.ToString(ContactType.Phone),
                    ContactSubType = Convert.ToString(phone?.PhoneType),
                    ContactValue = phone?.PhoneNumber,
                    CountryCode = phone?.CountryCode,
                    CountryWithCode = string.Empty,
                    Description = phone?.Description,
                    Primary = createCustomerEntities.EnrollmentChannel == Constants.Application.VREDChannelID && (createCustomerEntities?.ContactDetails?.PhoneDetails.Count == 1 || Convert.ToString(phone?.PhoneType) == Constants.Application.MobilePhoneType) ? "Y" : phone.PreferredFlag,
                    Usage = Convert.ToString(phone?.Usage),
                    Valid = "Y",
                    CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    LastUpdatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    SourceIdentifier = string.Empty,
                    SourceSystem = string.Empty,
                };
                phoneGrp.Add(phones);
            });
            return phoneGrp.ToArray();
        }
    }
}
