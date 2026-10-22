using Newtonsoft.Json;
using VAA.EAPI.HealthStatusApp.Helpers;
using VAA.EAPI.HealthStatusApp.Models;

public class CustomHealthReport
{
    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("statusCode")]
    public int StatusCode { get; set; }

    [JsonProperty("result")]
    [JsonConverter(typeof(ResultConverter))]
    public Result Result { get; set; }
}





