namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Application.PricingFeed
{
    public interface IGeneratePricingFeedService
    {
        public Task PublishAsync();
    }
}
