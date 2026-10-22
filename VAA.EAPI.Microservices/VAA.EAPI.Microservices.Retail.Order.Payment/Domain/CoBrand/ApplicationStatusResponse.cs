using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CoBrand
{
    public class ApplicationStatusResponse
    {
        public string DataShareToken { get; set; }
        public string ApplicationDecision { get; set; }
        public string CreditLimit { get; set; }
        public string TokenStatus { get; set; }
        public DateTime TokenCreationDate { get; set; }
    }
}
