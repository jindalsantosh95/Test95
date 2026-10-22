namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models
{
    public class Child
    {
        public IEnumerable<string> LocationId { get; set; }
        public string Description { get; set; }
        public IEnumerable<Child> Children { get; set; }

    }
}
