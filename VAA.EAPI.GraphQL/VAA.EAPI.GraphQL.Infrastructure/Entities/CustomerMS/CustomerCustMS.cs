using HotChocolate;
using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS
{
    public class CustomerCustMS
    {
        [GraphQLDescription("Customer ID")]
        public string CustomerId { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredFirstName")]
        [GraphQLDescription("Customer's preferred first name")]
        public string FirstName { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredLastName")]
        [GraphQLDescription("Customer Surname")]
        public string LastName { get; set; }
        [GraphQLDescription("Customer Middle Name")]
        public string MiddleName { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredGender")]
        [GraphQLDescription("Customer Gender")]
        public string Gender { get; set; }
        [Newtonsoft.Json.JsonProperty("preferredSalutation")]
        [GraphQLDescription("Customer's preferred salutation")]
        public string Salutation { get; set; }
        [GraphQLDescription("Customer's preferred pronouns")]
        public string Pronouns { get; set; }
        [GraphQLDescription("Customer's nationality")]
        public string Nationality { get; set; }
        [GraphQLDescription("Customer's loyalty member number")]
        public string LoyaltyNumber { get; set; }
        [GraphQLDescription("Customer's date of birth")]
        public DateTime DateOfBirth { get; set; }
        [GraphQLDescription("List of addresses for the customer")]
        public IEnumerable<AddressCustMS> AddressDetails { get; set; }
        [GraphQLDescription("Contact details for the customer including email and phone")]
        public Contact ContactDetails { get; set; }
        [GraphQLDescription("Title of  the customer")]
        public string Title { get; set; }
        [GraphQLDescription("Customer's FC account status")]
        public string Status { get; set; }
        [GraphQLDescription("Customer's FC account Type e.g. Individual, Household etc.")]
        public string MemberAccountType { get; set; }
        [GraphQLDescription("Customer's FC account Tier e.g. Gold, Red etc.")]
        public string MemberTier { get; set; }
        [GraphQLDescription("Member number of linked supplementary silver head account")]
        public string SuppSilverHeadAccount { get; set; }

        [GraphQLDescription("Head of household member number if given Customer is a child member of household")]
        public string HeadOfGroupMemberNumber { get; set; }

        [GraphQLDescription("Terms & Condition acceptance flag for FC account")]
        public string TermsAndConditionsAccepted { get; set; }
        [GraphQLDescription("customer Enrolled on")]
        public string CreatedOn { get; set; }

        [GraphQLDescription("customer Enrolled by")]
        public string CreatedBy { get; set; }
        [GraphQLDescription("List of awards for the customer")]
        public IEnumerable<AwardCustMS> AwardDetails { get; set; }
        public string PnrName { get; set; }

        public CustomerCustMS()
        {

        }
    }
}
