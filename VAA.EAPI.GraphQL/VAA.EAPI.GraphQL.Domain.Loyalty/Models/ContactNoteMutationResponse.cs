using HotChocolate;
using System.Collections.Generic;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Entities.Enums;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class ContactNoteMutationResponse
    {
        [GraphQLDescription("Mutation request ID")]
        public string RequestId { get; set; }
        [GraphQLDescription("Collection of any errors encountered during processing of Contact Note Mutation")]
        public List<ErrorDetails> Errors { get; set; }
        [GraphQLDescription("Mutated Contact Note")]
        public ContactNote ContactNote { get; set; }
        [GraphQLDescription("Outcome of Mutation request")]
        public Status CommandStatus { get; set; }
    }
}
