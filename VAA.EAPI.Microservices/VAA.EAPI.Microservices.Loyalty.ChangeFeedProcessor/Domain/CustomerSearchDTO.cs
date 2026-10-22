using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain
{
    public class CustomerSearchDTO : CosmosDocument
    {
        public string DateOfBirth { get; set; }

        public string MatchString { get; set; }

        public string FirstName { get; set; }

        public string Title { get; set; }

        public string CustomerId { get; set; }

        public string Gender { get; set; }

        public string MiddleName { get; set; }

        public string EmailAddress { get; set; }

        public string LoyaltyMemberNumber { get; set; }

        public string LastName { get; set; }

        public string DocType { get; set; }

        public string PostCode { get; set; }

        public string Country { get; set; }

        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;
        public string Province { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneCountryCode { get; set; }



        public CustomerSearchDTO()
        {
            //required for seriaization
        }
        public CustomerSearchDTO(CustomerSearch custSearch)
        {
            if (custSearch == null) throw new ArgumentNullException(nameof(custSearch));

            Id = custSearch.Id;
            PartitionKey = custSearch.DocType;//map to docType.
            DateOfBirth = custSearch.DateOfBirth;
            MatchString = custSearch.MatchString ?? string.Empty;
            FirstName = custSearch.FirstName ?? string.Empty;
            Title = custSearch.Title ?? string.Empty;
            CustomerId = custSearch.CustomerId ?? string.Empty;
            Gender = custSearch.Gender ?? string.Empty;
            MiddleName = custSearch.MiddleName ?? string.Empty;
            EmailAddress = custSearch.EmailAddress ?? string.Empty;
            LoyaltyMemberNumber = custSearch.LoyaltyMemberNumber ?? string.Empty;
            LastName = custSearch.LastName ?? string.Empty;
            DocType = custSearch.DocType ?? string.Empty;
            PostCode = custSearch.PostCode ?? string.Empty;
            Country = custSearch.Country ?? string.Empty;
            Province = custSearch.Province ?? string.Empty;
            PhoneCountryCode = custSearch.PhoneCountryCode ?? string.Empty;
            PhoneNumber = custSearch.PhoneNumber ?? string.Empty;
        }
    }
}
