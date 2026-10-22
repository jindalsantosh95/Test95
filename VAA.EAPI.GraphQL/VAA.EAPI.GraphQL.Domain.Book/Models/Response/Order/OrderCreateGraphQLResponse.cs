using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.BookGraphQLModels;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order
{
    public class OrderCreateGraphQLResponse
    {
        public PointOfSale PointOfSale { get; set; }
        public Summary Summary { get; set; }
        public int BookingReference { get; set; }
        public IEnumerable<string> Infants { get; set; }
        public IEnumerable<string> Children { get; set; }
        public Configuration Configuration { get; set; }
        public IEnumerable<string> Adults { get; set; }
        public IEnumerable<BasketPerson> People { get; set; }
        public LeadPassenger LeadPassenger { get; set; }
        public object LeadDriver { get; set; }
        public IEnumerable<PaymentInformation> PaymentInformation { get; set; }
        public object PayPal { get; set; }
        public string ResourceType { get; set; }
        public CustomerProfile CustomerProfile { get; set; }
        public bool NewVacCreated { get; set; }
        public CustomerProfile VACCustomer { get; set; }
        public FCEarnPoints EarnPoints { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
