using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class CorporateAccount
    {
        public string AccountName { get; set; }
        public string AgencyID { get; set; }
        public string CorporateCode { get; set; }
        public string TicketDesignator { get; set; }
        public bool ActiveContract { get; set; }
        public string CarrierCode { get; set; }

        public static explicit operator CorporateAccount(JObject source)
        {
            return new CorporateAccount()
            {
                AccountName = source["AccountName"]?.ToString(),
                AgencyID = source["AgencyID"]?.ToString(),
                CorporateCode = source["CorporateCode"]?.ToString(),
                TicketDesignator = source["TicketDesignator"]?.ToString(),
                ActiveContract = source["ActiveContract"]?.Value<bool?>() ?? false,
                CarrierCode = source["CarrierCode"]?.ToString(),
            };
        }
    }
}
