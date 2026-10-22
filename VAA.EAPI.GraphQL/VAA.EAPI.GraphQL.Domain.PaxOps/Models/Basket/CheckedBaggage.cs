namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Basket
{
    public class CheckedBaggage
    {
        public string ProductType { get; set; }
        public IEnumerable<PsgBaggageInfo> PassengerBaggage { get; set; }
    }
}
