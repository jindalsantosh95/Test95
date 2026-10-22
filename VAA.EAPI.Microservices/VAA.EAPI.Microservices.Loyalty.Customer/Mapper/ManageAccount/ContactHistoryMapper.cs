using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using Contact = VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.Contact;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount
{
    public class ContactHistoryMapper
    {
        public static ContactHistory EmailMapRequest(Customers createCustomerEntities)
        {
            IEnumerable<Contact> emailDetails = createCustomerEntities.ContactDetails.Where(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y");

            return new ContactHistory()
            {
                Id = Guid.NewGuid().ToString(),
                DocType = DocumentType.ContactHistory.ToString(),
                PartitionKey = createCustomerEntities.PartitionKey,
                CustomerId = createCustomerEntities.CustomerId,
                ContactId = emailDetails?.First().ContactId,
                SourceIdentifier = emailDetails?.First().SourceIdentifier,
                SourceSystem = emailDetails?.First().SourceSystem,
                CreatedOn = DateTime.Parse(emailDetails?.First().CreatedOn),
                CreatedBy = emailDetails?.First().CreatedBy,
                LastUpdatedOn = DateTime.Parse(emailDetails?.First().LastUpdatedOn),
                LastUpdatedBy = emailDetails?.First().LastUpdatedBy,
                ContactValue = emailDetails?.First().ContactValue,
                ContactType = emailDetails?.First().ContactType,
                ContactSubType = emailDetails?.First().ContactSubType,
                Usage = emailDetails?.First().Usage,
                Valid = emailDetails?.First().Valid,
                Description = emailDetails?.First().Description,
                CountryCode = emailDetails?.First().CountryCode,
                CountryWithCode = emailDetails?.First().CountryWithCode,
                Primary = emailDetails?.First().Primary
            };
        }

    }

}
