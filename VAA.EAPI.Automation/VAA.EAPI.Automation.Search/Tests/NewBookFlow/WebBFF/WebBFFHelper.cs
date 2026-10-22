using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using static VAA.EAPI.Automation.Core.Config;

namespace VAA.EAPI.Automation.Search.Tests.NewBookFlow.WebBFF
{
    public class WebBFFHelper
    {
        private static Random rnd = new Random();

        public static void SummaryURL(string url, out string basketId, out string slice, out string offerId)
        {
            basketId = url.Split('/')[5];
            slice = url.Split('/')[7];
            offerId = url.Split('/').LastOrDefault();
        }

        public static string BuildSummaryUrl(string basketId, string slice, string offerId) => $"{TestResourcesVirginAtlantic.BaseUrl}/{Config.Environment}/search/summary/{basketId}/{slice}|{offerId}";

        public static void ProductionSummaryUrl(string url, out string basketId, out string slice, out string offerId)
        {
            basketId = url.Split('/')[4];
            slice = url.Split('/')[6];
            offerId = url.Split('/').LastOrDefault();
        }

        public static string GetSortLinkBySortName(string jsonResponse, string sortName)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string sortLink = jsonObject.SelectToken($"$.result.flightSortLinks.links[?(@.name == '{sortName}')].href").ToString();

            return sortLink;
        }

        public static string GetFilterLinkByFilterName(string jsonResponse, string filterName)
        {
            JObject jsonObject = JObject.Parse(jsonResponse);

            string filterLink = jsonObject.SelectToken($"$.result.flightSearchFilters.filters[*].values[?(@.displayName == '{filterName}')].links[*].href").ToString();

            return filterLink;
        }
    }
}