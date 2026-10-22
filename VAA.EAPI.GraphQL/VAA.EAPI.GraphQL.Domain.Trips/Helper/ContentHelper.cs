
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Trips.Helper
{
    public class ContentHelpers
    {
        public static Dictionary<string, string> GetDestinationImageContent(IEnumerable<string> content)
        {
            if (content == null || !content.Any())
                return new Dictionary<string, string>();
            var destinationImageContent = new Dictionary<string, string>();
            var contentToken = JsonConvert.DeserializeObject<JToken>(content.First());
            var destinationCards = contentToken["destinationCards"];
            if (destinationCards != null && destinationCards.Type == JTokenType.Array)
            {
                var contentArray = (JArray)destinationCards;
                destinationImageContent = contentArray.ToDictionary(
                    item => item["iata"]?.ToString() ?? string.Empty, 
                    item => item["destinationImage"].HasValues ? item["destinationImage"]?["_publishUrl"]?.ToString() : string.Empty);
            }
            return destinationImageContent;
        }
    }
}
