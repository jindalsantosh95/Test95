using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Data;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CustomerResponse : CustomerCustMS
    {
        [UseSorting]
        [UseFiltering]
        [Authorize(Roles = new[] { "ContactNotes.Read" })]
        [GraphQLDescription("List of contact notes for the member")]
        public IEnumerable<Infrastructure.Entities.CustomerTransactions.ContactNote> ContactNotes { get; set; }
        public CustomerResponse()
        {

        }

        public CustomerResponse(CustomerCustMS response)
        {
            CustomerId = response.CustomerId;
            FirstName = response.FirstName;
            LastName = response.LastName;
            Gender = response.Gender;
            Salutation = response.Salutation;
            Pronouns = response.Pronouns;
            Nationality = response.Nationality;
            LoyaltyNumber = response.LoyaltyNumber;
            DateOfBirth = response.DateOfBirth;
            AddressDetails = response.AddressDetails;
            ContactDetails = response.ContactDetails;
            MemberAccountType = response.MemberAccountType;
            MemberTier = response.MemberTier;
            SuppSilverHeadAccount = response.SuppSilverHeadAccount;
            HeadOfGroupMemberNumber = response.HeadOfGroupMemberNumber;
            Status = response.Status;
            TermsAndConditionsAccepted = response.TermsAndConditionsAccepted;
            AwardDetails = response.AwardDetails;
        }
    }
}
