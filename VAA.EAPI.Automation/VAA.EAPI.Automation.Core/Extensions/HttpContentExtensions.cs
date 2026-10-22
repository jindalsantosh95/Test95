using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Core.Extensions
{
    public static class HttpContentExtensions
    {
        public static string ReadAsString(this HttpContent source)
        {
            if (source == null)
                return string.Empty;

            Task<string> task = source.ReadAsStringAsync();

            return task.Status == TaskStatus.RanToCompletion ? task.Result : task.GetAwaiter().GetResult();
        }

        public static T ReadJsonAs<T>(this HttpContent source)
        {
            if (source == null)
                return default;

            string json = source.ReadAsString();

            return json.IsNullOrWhiteSpace() ? default : JsonConvert.DeserializeObject<T>(json);
        }
    }
}
