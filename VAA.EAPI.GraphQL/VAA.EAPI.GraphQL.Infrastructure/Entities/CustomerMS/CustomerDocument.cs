using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS
{
    public class CustomerDocument
    {
        public DateTime CreatedOn { get; set; }

        public string CustomerId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string LoyaltyMemberNumber { get; set; }

        public IEnumerable<AddressCustMS> AddressDetails { get; set; }

        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public IEnumerable<ContactDetails> ContactDetails { get; set; }

    }
}
