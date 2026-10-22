using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Ticket
{
    public class ReasonForIssuance
    {
        public string RFIC { get; set; }
        public string RFISC { get; set; }

        public static explicit operator ReasonForIssuance(JObject source)
        {
            return new ReasonForIssuance()
            {
                RFIC = source["RFIC"]?.ToString(),
                RFISC = source["RFISC"]?.ToString()
            };
        }
    }
}
