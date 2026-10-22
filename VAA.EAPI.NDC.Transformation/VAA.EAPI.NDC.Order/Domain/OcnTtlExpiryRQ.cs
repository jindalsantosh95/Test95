using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Domain
{
    public class OcnTtlExpiryRQ
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReference { get; set; }
        public DataList DataList { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
    }
}