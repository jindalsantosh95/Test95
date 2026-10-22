using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Models
{
    public class CorporateDbEntity : TableEntity
    {
        [JsonProperty("CBA_CLID")]
        public string CbaClid { get; set; }

        [JsonProperty("CBI_Ticket_Designator")]
        public string CbiTicketDesignator { get; set; }

        [JsonProperty("CBA_VS_Active_Contract")]
        public bool CbaVsActiveContract { get; set; }

        [JsonProperty("ACC_Corporate_Name")]
        public string AccCorporateName { get; set; }

        [JsonProperty("CBI_Partner_Carrier_Code")]
        public string CbiPartnerCarrierCode { get; set; }

    }
}