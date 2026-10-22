using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public static class DocumentDetailMapper
    {
        public static DocumentDetail[] MapDocumentDetailToSCVDocument(CreateCustomerEntities createCustomerEntities)
        {
            return new DocumentDetail[]
            {
                new ()
                {
                    LastUpdatedBy = string.Empty,
                    LastUpdatedOn = string.Empty,
                    SourceIdentifier = string.Empty,
                    SourceSystem = string.Empty,
                    CreatedOn = DateTime.Now.ToString("yyyy-MM-dd"),
                    CreatedBy = createCustomerEntities?.ChannelUserId ?? createCustomerEntities.EnrollmentChannel,
                    DocumentId = string.Empty,
                    DocumentType = string.Empty,
                    DocumentNumber = string.Empty,
                    DocumentTitle = string.Empty,
                    DocumentFirstName = string.Empty,
                    DocumentLastName = string.Empty,
                    DocumentMiddleName = string.Empty,
                    IssuingCountry = string.Empty,
                    IssueDate = string.Empty,
                    ExpiryDate = string.Empty,
                    Nationality = string.Empty,
                    CountryOfResidence = string.Empty
                }
            };
        }
    }
}
