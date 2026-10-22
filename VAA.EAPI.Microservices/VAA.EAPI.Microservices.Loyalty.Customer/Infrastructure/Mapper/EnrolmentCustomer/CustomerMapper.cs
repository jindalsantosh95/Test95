using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class CustomerMapper
    {
        public static Customers MapEAPICustomerToSCVCustomer(CreateCustomerEntities createCustomerEntities, string docType, string authCustId)
        {
            if (createCustomerEntities == null)
            {
                throw new ArgumentNullException(nameof(createCustomerEntities));
            }

            string updatedLastName = GetLastName.PopulateLastName(createCustomerEntities);
            return new Customers()
            {
                InformalSalutation = $"Dear {createCustomerEntities.FirstName}",
                Gender = createCustomerEntities.Gender,
                EnvelopeSalutation = string.IsNullOrEmpty(createCustomerEntities.Title) ? $"{createCustomerEntities.FirstName} {updatedLastName}" : $"{createCustomerEntities.Title} {createCustomerEntities.FirstName} {updatedLastName}",
                FormalSalutation = string.IsNullOrEmpty(createCustomerEntities.Title) ? $"Dear {updatedLastName}" : $"Dear {createCustomerEntities.Title} {updatedLastName}",
                LoyaltyMemberNumber = createCustomerEntities.FcNumber,
                Id = createCustomerEntities.CustomerId,
                DocType = docType,
                Authorizations = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && (createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount) ? null : AuthorizationMapper.MapEAPIAuthorizationToSCVAuthorization(createCustomerEntities, authCustId),
                DateOfBirth = DateTimeHelper.ConvertDateTimeFormat(createCustomerEntities.DateOfBirth, Constants.Application.EAPIEnrolmentDOBFormat, Constants.Application.EAPIEnrolmentDOBFormat)?.ToString(Constants.Application.EAPIEnrolmentDOBFormat),
                MatchString = MatchString.GetMatchString(createCustomerEntities),
                AddressDetails = createCustomerEntities.EnrollmentChannel == Constants.Application.WebChannelID && (createCustomerEntities.CustomerType == CustomerTypeEnum.Basic || createCustomerEntities.CustomerType == CustomerTypeEnum.VirginAccount) ? null : AddressDetailMapper.MapEAPIAddressToSCVAddress(createCustomerEntities),
                FirstName = createCustomerEntities.FirstName,
                VisibleEnterpriseWide = string.Empty,
                ContactDetails = ContactDetailMapper.MapEAPIContactToSCVContact(createCustomerEntities),
                Citizenship = string.Empty,
                Title = createCustomerEntities.Title,
                CustomerId = createCustomerEntities.CustomerId,
                MiddleName = createCustomerEntities.MiddleName,
                DocumentDetails = null,
                NameChangeReason = string.Empty,
                Brand = Constants.Application.Brand,
                MaritalStatus = string.Empty,
                Active = createCustomerEntities.Status == StatusEnum.Active.ToString() ? "Y" : "N",
                PartitionKey = createCustomerEntities.CustomerId,
                LastName = updatedLastName,
                DeathCertificateNumber = string.Empty,
                DeceasedDate = string.Empty,
                PNRName = $"{RegExHelper.RemoveNonAlphaCharacters(updatedLastName)}/{RegExHelper.RemoveNonAlphaCharacters(createCustomerEntities.FirstName)}{createCustomerEntities.Title}".ToUpper(),
                SocialProfiles = null,
                SourceSystem = string.Empty,
                CreatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedOn = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                CreatedBy = createCustomerEntities.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                SourceIdentifier = string.Empty,
                SchemaVersion = Constants.Application.SchemaVersion,
                EnrollmentChannel = createCustomerEntities.EnrollmentChannel,
                Status = createCustomerEntities.Status
            };
        }
    }
}
