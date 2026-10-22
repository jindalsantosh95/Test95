using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS
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
