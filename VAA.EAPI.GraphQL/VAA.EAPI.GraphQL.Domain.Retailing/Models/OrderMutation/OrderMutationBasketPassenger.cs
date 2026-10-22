namespace VAA.EAPI.GraphQL.Domain.Retailing.Models.OrderMutation
{
    public class OrderMutationBasketPassenger
    {
        public string PassengerID { get; set; }
        public IEnumerable<SSR> SSRs { get; set; }
    }
}
