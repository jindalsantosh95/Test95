using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{
    public class OriginalTicketDocInfo
    {
        public string IssueDate { get; set; }
        public string DocumentNumber { get; set; }

        public static explicit operator OriginalTicketDocInfo(JObject source)
        {
            return new OriginalTicketDocInfo()
            {
                IssueDate = source["IssueDate"]?.ToString(),
                DocumentNumber = source["DocumentNumber"]?.ToString()
            };
        }
    }
}
