namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class Offer
    {
        public string ProductType { get; set; }
        public string ProductCode { get; set; }
        public IEnumerable<Service> Services { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
    }
}
