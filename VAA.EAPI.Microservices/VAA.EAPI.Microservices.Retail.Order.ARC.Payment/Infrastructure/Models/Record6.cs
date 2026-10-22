using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Infrastructure.Models
{
    public class Record6
    {
        [JsonProperty("FIRST_COUPON")] public Coupon FIRST_COUPON { get; set; }
        [JsonProperty("SECOND_COUPON")] public Coupon SECOND_COUPON { get; set; }
        [JsonProperty("THIRD_COUPON")] public Coupon THIRD_COUPON { get; set; }
    }
}
