using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class ExchangeDetails
    {
        public string OriginalTrackID { get; set; }
        public string RelatedTicketDocumentNumber { get; set; }
        public string RelatedTicketDocumentCouponNumberIdentifier { get; set; }
    }
}