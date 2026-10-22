using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VAA.Facade.EnrolmentFacade.Domain.CustomerDetails
{
    public class ContactDetails
    {
        [JsonPropertyName("emailDetails")]
        public IEnumerable<EmailDetails> EmailDetails { get; set; } 
    }
}
