namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class BaggageOffer
    {
        public int BaggageCount { get; set; }
        public string baggageOfferId { get; set; }
        public BaggagePrice Price { get; set; }
    }
}
