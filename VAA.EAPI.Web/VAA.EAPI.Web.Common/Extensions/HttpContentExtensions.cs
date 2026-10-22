using Newtonsoft.Json;
using System.Net.Http;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class HttpContentExtensions
    {
        public static string ReadAsString(this HttpContent source) => source is not null && source.ReadAsStream().Length > 0 ? source.ReadAsStringAsync().GetAwaiter().GetResult() : string.Empty;
        public static T ReadJsonAs<T>(this HttpContent source) => source is not null && source.ReadAsStream().Length > 0 ? JsonConvert.DeserializeObject<T>(source.ReadAsString()) : default;
    }
}
