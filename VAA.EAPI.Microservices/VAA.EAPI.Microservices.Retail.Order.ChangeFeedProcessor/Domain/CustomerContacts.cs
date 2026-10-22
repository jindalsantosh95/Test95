using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ChangeFeedProcessor.Domain
{
    public partial class CustomerContacts
    {
        [JsonProperty("ContactInfoID")]
        public string? ContactInfoId { get; set; }

        [JsonProperty("Contacts")]
        public IEnumerable<Contact> Contacts { get; set; }

        [JsonProperty("Addresses")]
        public IEnumerable<Address>? Addresses { get; set; }
    }
}
