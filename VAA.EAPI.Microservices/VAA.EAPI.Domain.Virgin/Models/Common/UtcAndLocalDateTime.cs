using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Common
{
    public class UtcAndLocalDateTime
    {
        public string UtcDateTime { get; set; }
        public string LocalDateTime { get; set; }

        public static explicit operator UtcAndLocalDateTime(JObject source)
        {
            return new UtcAndLocalDateTime()
            {
                UtcDateTime = source["UtcDateTime"]?.ToString(),
                LocalDateTime = source["LocalDateTime"]?.ToString()
            };
        }
    }
}
