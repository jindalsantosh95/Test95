using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain
{
    public class TtlExpiryRequest
    {
        public Pos Pos { get; set; }
        public Parties Parties { get; set; }
        public BookingReference BookingReference { get; set; }
        public DataList DataList { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }

        public static explicit operator TtlExpiryRequest((OrderCreateRequest orderCreateRequest, OrderDocument orderDocument) source)
        {
            var (orderCreateRequest, orderDocument) = source;

            System.ArgumentNullException.ThrowIfNull(orderCreateRequest);
            System.ArgumentNullException.ThrowIfNull(orderDocument);

            return new TtlExpiryRequest
            {
                Pos = orderCreateRequest.Pos,
                Parties = orderCreateRequest.Parties,
                TimeLimits = orderCreateRequest.TimeLimits,
                BookingReference = orderDocument.BookingReference,
                DataList = orderDocument.DataList
            };
        }
    }
}
