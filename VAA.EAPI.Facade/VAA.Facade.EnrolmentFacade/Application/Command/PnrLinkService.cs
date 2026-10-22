using System.Text.Json.Serialization;
using VAA.Facade.EnrolmentFacade.Domain;

namespace VAA.Facade.EnrolmentFacade.Application.Command
{
    public class PnrLinkService(CustomerEnrolmentDetails customerDetails)
    {
        [JsonPropertyName("customer")]
        public CustomerEnrolmentDetails Customer { get; set; } = customerDetails;
    }
}
