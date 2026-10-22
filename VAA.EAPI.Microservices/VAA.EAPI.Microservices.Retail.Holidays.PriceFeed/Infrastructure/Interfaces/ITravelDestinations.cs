namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Interfaces
{
    public interface ITravelDestinations
    {
        Task<IEnumerable<string>> RetriveTravelDestinations();
    }
}
