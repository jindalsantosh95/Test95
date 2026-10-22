using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class RefundDetails
    {
        public string DataInputStatusIndicator { get; set; }
        public string RefundDocumentNumber { get; set; }
        public string RelatedTicketDocumentCouponNumberIdentifier { get; set; }
        public string RelatedTicketDocumentNumber { get; set; } 
    }
}
