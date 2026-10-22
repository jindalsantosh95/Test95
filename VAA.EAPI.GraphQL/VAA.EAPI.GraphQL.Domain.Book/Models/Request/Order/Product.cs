using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    [GraphQLName("insuranceProduct")]
    public class Product
    {
        public int Sequence { get; set; }
        public string QuoteID { get; set; }
        public string ProductID { get; set; }
        public InsuredDetails InsuredDetails { get; set; }
        public double CoverageAmount { get; set; }
        public double Price { get; set; }
        public double PremiumPrice { get; set; }
        public double CharitableContribution { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}