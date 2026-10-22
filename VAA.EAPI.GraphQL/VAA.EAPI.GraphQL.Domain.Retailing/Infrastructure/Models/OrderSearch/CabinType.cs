using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.CabinType")]
    public class CabinType
    {
        public string CabinCode { get; set; }
        public string CabinName { get; set; }
    }
}