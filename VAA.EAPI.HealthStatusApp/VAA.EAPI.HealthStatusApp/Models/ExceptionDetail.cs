using Newtonsoft.Json;

namespace VAA.EAPI.HealthStatusApp.Models
{
    public class ExceptionDetail
    {
        [JsonProperty("response")]
        public Response Response { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("hResult")]
        public int HResult { get; set; }

        [JsonProperty("stackTrace")]
        public string StackTrace { get; set; }
    }

}
