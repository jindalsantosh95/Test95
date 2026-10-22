using HotChocolate;
using System.Collections.Generic;
namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS
{
    public class Contact
    {
        [GraphQLDescription("List of contact for the member")]
        public List<Email> EmailDetails { get; set; }
        public List<Phone> PhoneDetails { get; set; }
    }
}
