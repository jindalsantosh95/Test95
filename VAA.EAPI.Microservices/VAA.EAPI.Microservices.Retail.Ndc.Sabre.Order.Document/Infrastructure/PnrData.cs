using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Document.Infrastructure
{
    public class PnrData
    {
        public string Rloc { get; set; }

        public static explicit operator PnrData(IssueTicketResponse response)
        {
            return new()
            {
                Rloc = response.BookingReference.RLOC
            };
        }
    }
}