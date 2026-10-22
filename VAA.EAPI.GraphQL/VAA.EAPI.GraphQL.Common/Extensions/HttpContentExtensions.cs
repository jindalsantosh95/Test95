using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Common.Extensions
{
    public static class HttpContentExtensions
    {
        public static string ReadAsString(this HttpContent source)
        {
            if (source == null) return string.Empty;

            var task = source.ReadAsStringAsync();

            return task.Status == TaskStatus.RanToCompletion ? task.Result : task.GetAwaiter().GetResult();
        }

        public static T ReadJsonAs<T>(this HttpContent source)
        {
            if (source == null) return default;

            var json = source.ReadAsString();

            return json.IsNullOrWhiteSpace() ? default : JsonConvert.DeserializeObject<T>(json);
        }
    }
}
