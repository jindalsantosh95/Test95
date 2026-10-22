namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class Basket
    {
        public Guid BasketId { get; set; }
        public PnrDetails BookingDetails { get; set; }
        public CheckedBaggage CheckedBaggageDetails { get; set; }
        public ContactRecord ContactDetails { get; set; }
    }
}
