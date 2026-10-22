using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.CustomerDetail")]
    public class CustomerDetail
    {        
        public string CustId { get; set; }
        public string PTC { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastNameNumber { get; set; }       
        public LoyaltyMemberDetails LoyaltyMemberDetails { get; set; }
       
    }
}