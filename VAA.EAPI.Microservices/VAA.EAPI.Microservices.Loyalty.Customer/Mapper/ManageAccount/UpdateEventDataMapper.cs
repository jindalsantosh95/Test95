using System;
using System.Linq;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using static VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities.PhoneDetails;
using VAA.EAPI.Common.Helpers;
namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount
{
    public class UpdateEventDataMapper
    {
        public static CloudEvent EventDataMapperB2C(CustomerSearch createUpdateEventRequest, string currentEmail)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinB2C,
                Data = new B2CEventRequest
                {
                    MemberNumber = createUpdateEventRequest.LoyaltyMemberNumber,
                    CurrentEmailId = currentEmail,
                    NewEmailId = createUpdateEventRequest.EmailAddress
                },
                Source = Constants.Application.CustomerEventHandlerTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinB2C,
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent EmailEventDataMapper(string EmailAddress)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubject,
                Data = new ManageAccountEmailMsRequest
                {
                    To = EmailAddress,
                    Template = ConfigReader.Get(Constants.Environment.AddressUpdateTemplate)
                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubject,
                Specversion = Constants.Application.DataVersion
            };
        }


        public static CloudEvent EventDataMapperVRED(Customers createUpdateEventRequest)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinVRED,
                Time = DateTime.UtcNow.ToString(),
                Data = new VREDEventRequest
                {
                    CustomerId = createUpdateEventRequest.CustomerId,
                    MemberNumber = createUpdateEventRequest.LoyaltyMemberNumber,
                    MemberStatus = createUpdateEventRequest.Status,
                    FirstName = createUpdateEventRequest.FirstName,
                    LastName = createUpdateEventRequest.LastName,
                    EmailId = createUpdateEventRequest.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y").ContactValue,
                    Channel = createUpdateEventRequest.EnrollmentChannel,
                    CreateUpdateTimestamp = DateTime.Parse(createUpdateEventRequest.CreatedOn).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinVRED,
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent EventDataMapperOCDB(Customers createUpdateEventRequest, string countryCode)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinOCDB,
                Time = DateTime.UtcNow.ToString(),
                Data = new OCDBEventRequest<AddressDetails>
                {
                    FirstName = createUpdateEventRequest.FirstName,
                    LastName = createUpdateEventRequest.LastName,
                    FcNumber = createUpdateEventRequest.LoyaltyMemberNumber,
                    AddressDetails = createUpdateEventRequest.AddressDetails.Where(x => x.Primary == "Y").Select(add => new AddressDetails
                    {
                        AddressLine1 = add.AddressLine1,
                        AddressLine2 = add.AddressLine2,
                        AddressLine3 = add.AddressLine3,
                        AddressType = add.AddressType,
                        PreferredFlag = add.Primary,
                        City = add.City,
                        CountryCode = countryCode,
                        Region = add.Province,
                        PostalCode = add.PostCode
                    }).ToList(),

                    ContactDetails = new ContactDetails
                    {
                        PhoneDetails = createUpdateEventRequest.ContactDetails?.Where(x => x.ContactType == Constants.Application.PhoneContactType && x.Primary == "Y").Select(ph => new PhoneDetails
                        {
                            PhoneNumber = ph.ContactValue,
                            CountryCode = ph.CountryCode,
                            PhoneType = ph.ContactType == PhoneTypes.Landline.ToString() ? PhoneTypes.Landline : PhoneTypes.Mobile,
                            PreferredFlag = ph.Primary
                        }).ToList(),

                        EmailDetails = createUpdateEventRequest.ContactDetails?.Where(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y").Select(em => new EmailDetails
                        {
                            EmailId = createUpdateEventRequest.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y").ContactValue,
                            Usage = em.Usage == Usages.Business.ToString() ? EmailDetails.Usages.Business : EmailDetails.Usages.Personal,
                            PreferredFlag = em.Primary
                        }).ToList()
                    },
                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinOCDB,
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent EventDataMapperEmailMS(Customers createUpdateEventRequest, string emailAddress)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubject,
                Time = DateTime.UtcNow.ToString(),
                Data = new EmailTriggerRequestManageAccount
                {
                    To = emailAddress,
                    Template = Constants.Application.PhoneUpdateTemplate
                },
                Source = Constants.Application.ManagaeAccountUpdateTopicName,
                Type = Constants.Application.UpdateCustomerEventSubject,
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
