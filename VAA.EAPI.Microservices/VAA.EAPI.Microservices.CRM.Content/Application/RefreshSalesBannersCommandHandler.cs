using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.CRM.Content.Domain.Aem;
using VAA.EAPI.Microservices.CRM.Content.Domain.SalesBanner;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;

namespace VAA.EAPI.Microservices.CRM.Content.Application
{
    public sealed class RefreshSalesBannersCommandHandler : IRefreshSalesBannersCommandHandler
    {
        private readonly IDistributedCacheService _distributedCacheService;
        private readonly IContentRequestHandler _contentRequestHandler;

        public static Dictionary<string, SalesBanner[]> SalesBanners { get; set; }

        public RefreshSalesBannersCommandHandler(IDistributedCacheService distributedCacheService, IContentRequestHandler contentRequestHandler)
        {
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
            _contentRequestHandler = contentRequestHandler ?? throw new ArgumentNullException(nameof(contentRequestHandler));
        }

        public async Task Handle(CancellationToken cancellationToken)
        {
            var response = await _contentRequestHandler.Handle("vaa-sales-banners", cancellationToken);

            var aemData = response.ToObject<AemSalesBannerList>();

            if (aemData == null) throw new NullReferenceException(nameof(aemData));

            var groupedAemSalesBanners = aemData.Items.GroupBy(s => s.DisplayLocation).ToList();

            foreach (var group in groupedAemSalesBanners)
            {
                var groupedSalesBanners = group
                    // Don't bother caching banners that could never be shown.
                    .Where(sb => sb.Enabled && (sb.DisplayUntil == null || sb.DisplayUntil > DateOnly.FromDateTime(DateTime.UtcNow)))
                    .Select(sb => (SalesBanner)sb);
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(groupedSalesBanners);

                // These should effectively never expire from the cache, as the trigger to pull from AEM should be called frequently.
                // Setting a high expiry just to clean up any types of banner that are no longer used.
                await _distributedCacheService.Set("sales-banners:" + group.Key, json, false, TimeSpan.Zero, TimeSpan.FromDays(30));
            }
        }
    }
}
