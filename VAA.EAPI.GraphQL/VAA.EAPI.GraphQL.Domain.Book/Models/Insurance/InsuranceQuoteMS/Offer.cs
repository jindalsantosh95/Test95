namespace VAA.EAPI.GraphQL.Domain.Book.Models.Insurance.InsuranceQuoteMS
{
    public class Offer
    {
        public string ContentCode { get; set; }
        public string ContentType { get; set; }
        public string Id { get; set; }
        public int Sequence { get; set; }
        public IEnumerable<Message>? Messages { get; set; }
    }
}