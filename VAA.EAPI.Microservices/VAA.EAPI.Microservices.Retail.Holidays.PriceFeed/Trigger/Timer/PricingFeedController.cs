using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Application.PricingFeed;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Trigger.Timer;

public sealed class PricingDataFeedTimerFunction(IGeneratePricingFeedService generatePricingFeedService)
{
    private readonly IGeneratePricingFeedService _generatePricingFeedService = generatePricingFeedService;

    [Function("PricingDataFeedTimerFunction")]
    public async Task Run([TimerTrigger("%PricingFeedSchedule%", UseMonitor = false)] TimerInfo timerInfo)
    {
        await _generatePricingFeedService.PublishAsync();
    }
}
