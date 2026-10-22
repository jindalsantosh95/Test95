using HotChocolate;
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class SupplementarySilverMemberAccount
    {
        [GraphQLDescription("The Member Number")]
        public string MemberNumber { get; set; }
        [GraphQLDescription("The Customer ID")]
        public string CustomerId { get; set; }
        [GraphQLDescription("SSA Association Date")]
        public DateTime? AssociationDate { get; set; }
        [GraphQLDescription("Status of a Member")]
        public string MemberStatus { get; set; }
        [GraphQLDescription("The First Name of Customer")]
        public string FirstName { get; set; }
        [GraphQLDescription("The Last Name of Customer")]
        public string LastName { get; set; }
        [GraphQLDescription("The relation ship type of Account")]
        public string RelationShipType { get; set; }
    }
}