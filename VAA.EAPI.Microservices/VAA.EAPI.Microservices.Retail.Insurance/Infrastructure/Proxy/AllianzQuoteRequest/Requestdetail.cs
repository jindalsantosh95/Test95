using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteRequest
{

    public class Requestdetail
    {
        public string BusinessUnit { get; set; }
        public string Currency { get; set; }
        public string Language { get; set; }
        public int OfferVersionId { get; set; }
        public string PaymentMethod { get; set; }
        public string RedemptionPoints { get; set; }
        public string[] RequestCategories { get; set; }
        public string RequestChannel { get; set; }
        public DateTime RequestedDate { get; set; }
        public string RequestOrigin { get; set; }
        public string SessionId { get; set; }
        public string TransactionId { get; set; }
    }


}
