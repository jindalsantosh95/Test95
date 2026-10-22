using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Aem;
using VAA.EAPI.Microservices.Retail.Reference.Domain.SalesBanner;

namespace VAA.EAPI.Microservices.Retail.Reference.Application
{
    public class RefreshSalesBannersCommandHandler : IRequestHandler<RefreshSalesBannersCommand>
    {
        private readonly IHttpConnector _httpConnector;
        private readonly IDistributedCacheService _distributedCacheService;

        public static Dictionary<string, SalesBanner[]> SalesBanners { get; set; }

        public RefreshSalesBannersCommandHandler(IHttpConnector httpConnector, IDistributedCacheService distributedCacheService)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task Handle(RefreshSalesBannersCommand request, CancellationToken cancellationToken)
        {
            var response = await _httpConnector.GetAsync(uri: ConfigReader.Get(Constants.Environment.AemGraphEndpointUri), headers: new Dictionary<string, string>(), null);

            response.EnsureSuccessStatusCode();

            var aemData = Json.Deserialize<AemSalesBannersResponse>(await response.Content.ReadAsStringAsync());
            if (aemData == null)
            {
                throw new NullReferenceException(nameof(aemData));
            }

            var groupedAemSalesBanners = aemData.Data.SalesBannerList.Items.GroupBy(s => s.DisplayLocation).ToList();
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
