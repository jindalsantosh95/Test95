using Newtonsoft.Json;

namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class TermsAndConditions
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Accepted { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
    }
}
