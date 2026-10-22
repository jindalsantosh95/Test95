using System.Text.Json.Serialization;

namespace VAA.Facade.EnrolmentFacade.Domain.CustomerDetails
{
    public class EmailDetails
    {
        [JsonPropertyName("EmailAddress")]
        public string EmailAddress { get; set; }
        [JsonPropertyName("Flag_to_indicate_if_preferred_email")]
        public string PreferredFlag { get; set; }
    }
}
