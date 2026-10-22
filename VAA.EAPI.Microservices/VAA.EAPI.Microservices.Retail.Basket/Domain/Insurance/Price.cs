
namespace VAA.EAPI.Microservices.Retail.Basket.Domain.Insurance
{   
    public class Price
    {
        public bool IsAvailable { get; set; }
        public bool IsIncluded { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
    }

}