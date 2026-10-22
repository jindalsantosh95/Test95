using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
  public class PaymentAuthorizeResponse
    {
        public string Status { get; set; }
        public int HttpStatus { get; set; }
        public string TrackID { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public IEnumerable<object> PspMessage { get; set; }
        public IEnumerable<object> PspCode { get; set; }
        public string PaymentStatus { get; set; }
        public AmountAuthorized AmountAuthorized { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
    }
    

}