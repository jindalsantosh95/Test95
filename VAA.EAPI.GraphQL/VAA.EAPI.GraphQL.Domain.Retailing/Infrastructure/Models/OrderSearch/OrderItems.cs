using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.OrderItems")]
    public class OrderItems
    {
        public string OrderItemId { get; set; }
        public List<Service> Services { get; set; }        
        public List<string> ServiceBeneficiaries { get; set; }
     
    }

}
