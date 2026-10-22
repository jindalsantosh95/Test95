namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class CardHolderDetail
    {
        public string CardHolderFirstName { get; set; }
        public string CardHolderLastName { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CardVerificationValue { get; set; }
    }
}
