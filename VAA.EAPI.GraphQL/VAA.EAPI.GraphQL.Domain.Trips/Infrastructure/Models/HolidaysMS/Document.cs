using Newtonsoft.Json.Linq;
using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public sealed class Document
    {
        public string Name { get; set; }

        public static explicit operator Document(HttpResponseMessage source)
        {
            if (source == null) return default;
            if (!source.IsSuccessStatusCode) return default;

            var content = source.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var json = Json.Deserialize<JObject>(content);

            return new()
            {
            };
        }
    }
}
