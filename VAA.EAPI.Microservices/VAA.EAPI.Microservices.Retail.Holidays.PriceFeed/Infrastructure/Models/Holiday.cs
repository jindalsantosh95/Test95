namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models
{
    public class Holiday
    {
        public Hotel Hotel { get; set; }
        public double PricePerPerson { get; set; }
        public SpecialOffers SpecialOffers { get; set; }
    }
}
