using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class OrderReference
    {
        public string DocType { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public DataList DataList { get; set; }
        public PointOfSale PointOfSale { get; set; }
        public UtcAndLocalDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public Parties Parties { get; set; }
        public string ChangeType { get; set; }
        public int? Version { get; set; }
        public UtcAndLocalDateTime ChangeDateTime { get; set; }
        public bool? IsPostSaleSSR { get; set; }
        public bool? IsSplit { get; set; }

        public static explicit operator OrderReference(OrderDocument source) =>
            source is null
                ? null
                : new OrderReference
                {
                    DocType = source.DocType,
                    FareTypeRequest = source.FareTypeRequest,
                    DataList = source.DataList is null ? null : (DataList)source.DataList,
                    PointOfSale = source.PointOfSale is null ? null : (PointOfSale)source.PointOfSale,
                    CreationDateTime = source.CreationDateTime,
                    OrderNumber = source.OrderNumber,
                    BookingReference = source.BookingReference,
                    TimeLimits = source.TimeLimits,
                    OrderItems = source.OrderItems?
                        .Where(orderItem => orderItem is not null)
                        .Select(orderItem => (OrderItem)orderItem),
                    OrderStatus = source.OrderStatus,
                    Parties = source.Parties is null ? null : (Parties)source.Parties,
                    ChangeType = source.ChangeType?.ToString(),
                    Version = source.Version,
                    ChangeDateTime = source.ChangeDateTime,
                    IsPostSaleSSR = source.IsPostSaleSSR
                };
    }
}
