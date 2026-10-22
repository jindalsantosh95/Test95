using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain
{
    public class OrderCreateRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public IEnumerable<Offers> SelectedOffers { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public DataList DataList { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<Remark> OrderRemarks { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
    }
}
