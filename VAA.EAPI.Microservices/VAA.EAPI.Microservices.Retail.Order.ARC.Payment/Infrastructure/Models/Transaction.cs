using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Transaction
    {
        [JsonProperty("validatingCarrier")]
        public string ValidatingCarrier { get; set; }

        [JsonProperty("currencyType")]
        public string CurrencyType { get; set; }

        [JsonProperty("record2")]
        public Record2 Record2 { get; set; }

        [JsonProperty("record3")]
        public Record3 Record3 { get; set; }

        [JsonProperty("record4")]
        public Record4 Record4 { get; set; }

        [JsonProperty("record5")]
        public IEnumerable<Record5> Record5 { get; set; }

        [JsonProperty("record6")]
        public IEnumerable<Record6> Record6 { get; set; }

        [JsonProperty("record7")]
        public IEnumerable<Record7> Record7 { get; set; }

        [JsonProperty("record8")]
        public IEnumerable<Record8> Record8 { get; set; }

        [JsonProperty("record9")]
        public Record9 Record9 { get; set; }

        [JsonProperty("recordA")]
        public RecordA RecordA { get; set; }

        [JsonProperty("recordG")]
        public IEnumerable<RecordG> RecordG { get; set; }

        [JsonProperty("conjunctions")]
        public IEnumerable<Conjunctions> Conjunctions { get; set; }
    }

}
