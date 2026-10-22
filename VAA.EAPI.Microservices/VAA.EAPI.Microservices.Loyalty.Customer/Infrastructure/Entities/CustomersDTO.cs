using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class CustomersDTO : CosmosDocument
    {
        public string Active { get; set; }

        public string CreatedOn { get; set; }

        public string LastUpdatedOn { get; set; }

        public Address[] AddressDetails { get; set; }

        public Authorization[] Authorizations { get; set; }

        public string Brand { get; set; }

        public string Citizenship { get; set; }

        public Contact[] ContactDetails { get; set; }

        public string CreatedBy { get; set; }


        public string CustomerId { get; set; }

        public string DateOfBirth { get; set; }

        public string DeathCertificateNumber { get; set; }

        public string DeceasedDate { get; set; }

        public string DocType { get; set; }

        public DocumentDetail[] DocumentDetails { get; set; }

        public string EnvelopeSalutation { get; set; }

        public string FirstName { get; set; }

        public string FormalSalutation { get; set; }

        public string Gender { get; set; }

        public string InformalSalutation { get; set; }

        public string LastName { get; set; }

        public string LastUpdatedBy { get; set; }

        public string LoyaltyMemberNumber { get; set; }

        public string MaritalStatus { get; set; }

        public string MatchString { get; set; }

        public string MiddleName { get; set; }

        public string NameChangeReason { get; set; }

        public string PNRName { get; set; }

        public SocialProfile[] SocialProfiles { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string Title { get; set; }

        public string VisibleEnterpriseWide { get; set; }

        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;

        public string EnrollmentChannel { get; set; }

        public string Status { get; set; }

        public CustomersDTO()
        {
            //required for serialization
        }
        public CustomersDTO(Customers cust)
        {
            if (cust == null) throw new ArgumentNullException(nameof(cust));

            Id = cust.Id;
            PartitionKey = cust.PartitionKey;
            CreatedOn = !string.IsNullOrEmpty(cust.CreatedOn) ? DateTime.Parse(cust.CreatedOn).ToString(Constants.Application.ISODateTimeFormat) : string.Empty;
            LastUpdatedOn = !string.IsNullOrEmpty(cust.LastUpdatedOn) ? DateTime.Parse(cust.LastUpdatedOn).ToString(Constants.Application.ISODateTimeFormat) : DateTime.Now.ToString(Constants.Application.ISODateTimeFormat);
            Active = cust.Active ?? string.Empty;
            if (cust.AddressDetails is not null)
            {
                IEnumerable<Address> addressDetails = cust.AddressDetails.Select(address => new Address
                {
                    AdditionalComments = address.AdditionalComments,
                    AddressId = address.AddressId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    AddressLine3 = address.AddressLine3,
                    AddressType = address.AddressType,
                    AddressVerified = address.AddressVerified,
                    City = address.City,
                    Country = address.Country,
                    CreatedBy = address.CreatedBy,
                    CreatedOn = !string.IsNullOrEmpty(address.CreatedOn) ? DateTime.Parse(address.CreatedOn).ToString(Constants.Application.ISODateTimeFormat) : string.Empty,
                    LandlordOrCompanyName = address.LandlordOrCompanyName,
                    LastUpdatedBy = address.LastUpdatedBy,
                    LastUpdatedOn = !string.IsNullOrEmpty(address.LastUpdatedOn) ? DateTime.Parse(address.LastUpdatedOn).ToString(Constants.Application.ISODateTimeFormat) : DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    MetroArea = address.MetroArea,
                    PostalStatus = address.PostalStatus,
                    PostCode = address.PostCode,
                    Primary = address.Primary,
                    Province = address.Province,
                    SourceIdentifier = address.SourceIdentifier,
                    SourceSystem = address.SourceSystem,
                    Valid = address.Valid,
                    GeoCodeValidFlag = address.GeoCodeValidFlag,
                    AddressVerificationStatus = address.AddressVerificationStatus,
                    GroupAddress = address.GroupAddress,
                    PartnerFileId = address.PartnerFileId
                });
                AddressDetails = addressDetails.ToArray();
            }
            if (cust.Authorizations is not null)
            {
                IEnumerable<Authorization> authorizations = cust.Authorizations.Select(authorization => new Authorization
                {
                    AuthorizerCustomerId = authorization.AuthorizerCustomerId,
                    CountryCode = authorization.CountryCode,
                    AuthorizerDateOfBirth = !string.IsNullOrEmpty(authorization.AuthorizerDateOfBirth) ? DateTime.Parse(authorization.AuthorizerDateOfBirth).ToString(Constants.Application.ISODateTimeFormat) : string.Empty,
                    Email = authorization.Email,
                    Name = authorization.Name,
                    Phone = authorization.Phone,
                    RelationType = authorization.RelationType,
                    AuthorizerFCMemberNumber = authorization.AuthorizerFCMemberNumber,
                    AuthorizationId = authorization.AuthorizationId,
                    SourceIdentifier = authorization.SourceIdentifier,
                    SourceSystem = authorization.SourceSystem,
                    Consents = authorization.Consents?.Select(consent => new Consent
                    {
                        ConsentFor = consent.ConsentFor,
                        ConsentProcessedDate = !string.IsNullOrEmpty(consent.ConsentProcessedDate) ? DateTime.Parse(consent.ConsentProcessedDate).ToString(Constants.Application.ISODateTimeFormat) : string.Empty,
                        EffectiveFrom = consent.EffectiveFrom,
                        EffectiveTo = consent.EffectiveTo,
                        LastUpdatedBy = consent.LastUpdatedBy,
                        LastUpdatedOn = !string.IsNullOrEmpty(consent.LastUpdatedOn) ? DateTime.Parse(consent.LastUpdatedOn).ToString(Constants.Application.ISODateTimeFormat) : DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                        TermsAndConditionsAccepted = consent.TermsAndConditionsAccepted,
                        CreatedBy = consent.CreatedBy,
                        CreatedOn = !string.IsNullOrEmpty(consent.CreatedOn) ? DateTime.Parse(consent.CreatedOn).ToString(Constants.Application.ISODateTimeFormat) : string.Empty,
                    }).ToArray()
                });
                Authorizations = authorizations.ToArray();
            }
            Brand = cust.Brand ?? string.Empty;
            Citizenship = cust.Citizenship ?? string.Empty;
            if (cust.ContactDetails is not null)
            {
                IEnumerable<Contact> contacts = cust.ContactDetails.Select(contact => new Contact
                {
                    ContactId = contact.ContactId,
                    ContactSubType = contact.ContactSubType,
                    ContactType = contact.ContactType,
                    ContactValue = contact.ContactValue,
                    CountryCode = contact.CountryCode,
                    CountryWithCode = contact.CountryWithCode,
                    CreatedBy = contact.CreatedBy,
                    CreatedOn = !string.IsNullOrEmpty(contact.CreatedOn) ? DateTime.Parse(contact.CreatedOn).ToString(Constants.Application.ISODateTimeFormat) : string.Empty,
                    Description = contact.Description,
                    LastUpdatedBy = contact.LastUpdatedBy,
                    LastUpdatedOn = !string.IsNullOrEmpty(contact.LastUpdatedOn) ? DateTime.Parse(contact.LastUpdatedOn).ToString(Constants.Application.ISODateTimeFormat) : DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                    Primary = contact.Primary,
                    SourceIdentifier = contact.SourceIdentifier,
                    SourceSystem = contact.SourceSystem,
                    Usage = contact.Usage,
                    Valid = contact.Valid
                });
                ContactDetails = contacts.ToArray();
            }
            CreatedBy = cust.CreatedBy ?? string.Empty;
            CustomerId = cust.CustomerId ?? string.Empty;
            DateOfBirth = !string.IsNullOrEmpty(cust.DateOfBirth) ? DateTime.Parse(cust.DateOfBirth).ToString(Constants.Application.ISODateTimeFormat) : string.Empty;
            DeathCertificateNumber = cust.DeathCertificateNumber ?? string.Empty;
            DeceasedDate = !string.IsNullOrEmpty(cust.DeceasedDate) ? DateTime.Parse(cust.DeceasedDate).ToString(Constants.Application.ISODateTimeFormat) : string.Empty;
            DocType = cust.DocType ?? string.Empty;
            DocumentDetails = cust.DocumentDetails;
            EnvelopeSalutation = cust.EnvelopeSalutation ?? string.Empty;
            FirstName = cust.FirstName ?? string.Empty;
            FormalSalutation = cust.FormalSalutation ?? string.Empty;
            Gender = cust.Gender ?? string.Empty;
            InformalSalutation = cust.InformalSalutation ?? string.Empty;
            LastName = cust.LastName ?? string.Empty;
            LastUpdatedBy = cust.LastUpdatedBy ?? string.Empty;
            LoyaltyMemberNumber = cust.LoyaltyMemberNumber ?? string.Empty;
            MaritalStatus = cust.MaritalStatus ?? string.Empty;
            MatchString = cust.MatchString ?? string.Empty;
            MiddleName = cust.MiddleName ?? string.Empty;
            NameChangeReason = cust.NameChangeReason ?? string.Empty;
            PNRName = cust.PNRName ?? string.Empty;
            SocialProfiles = cust.SocialProfiles;
            SourceIdentifier = cust.SourceIdentifier ?? string.Empty;
            SourceSystem = cust.SourceSystem ?? string.Empty;
            Title = cust.Title ?? string.Empty;
            VisibleEnterpriseWide = cust.VisibleEnterpriseWide ?? string.Empty;
            EnrollmentChannel = cust.EnrollmentChannel ?? string.Empty;
            Status = cust.Status ?? string.Empty;
        }
    }
}
