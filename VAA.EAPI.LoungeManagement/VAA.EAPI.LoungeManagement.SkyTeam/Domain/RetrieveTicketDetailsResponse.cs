using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain
{
    public class RetrieveTicketDetailsResponse
    {
        [JsonProperty("TrackId")]
        public string TrackId {  get; set; }

        [JsonProperty("EticketData")]
        public EticketData EticketData {  get; set; }

        [JsonProperty("ErrorOrAccessDenial")]
        public ErrorOrAccessDenial ErrorOrAccessDenial { get; set; }

    }

}