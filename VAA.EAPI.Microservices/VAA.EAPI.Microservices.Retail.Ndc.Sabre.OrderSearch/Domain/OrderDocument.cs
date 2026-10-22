using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain
{
    public class OrderDocument : CosmosDocument
    {
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public FareTypeRequest FareTypeRequest { get; set; }
        public DataList DataList { get; set; }
        public Pos PointOfSale { get; set; }
        public UtcAndLocalDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
        public Parties Parties { get; set; }
        public IEnumerable<Remark> OrderRemarks { get; set; }
        public IEnumerable<OrderHistory> OrderHistory { get; set; }
        public ChangeTypeEnum? ChangeType { get; set; }
        public int Version { get; set; }
        public UtcAndLocalDateTime ChangeDateTime { get; set; }
        public bool IsPostSaleSSR { get; set; }
        public int? ReshopHoldBookingCounter { get; set; }

    }
}
