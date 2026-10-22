using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Order
{
    public class OrderDocument : CosmosDocument
    {
        public string DocType { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DataList DataList { get; set; }
        public Pos PointOfSale { get; set; }
        public UtcAndLocalDateTime CreationDateTime { get; set; }
        public OrderNumber OrderNumber { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<TimeLimit> TimeLimits { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<OrderItems> OrderItems { get; set; }
        public string OrderStatus { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }       
        public Parties Parties { get; set; }
        public IEnumerable<Remark> OrderRemarks { get; set; }
        public string BookingType { get; set; }
        public string OrderType { get; set; }
        public bool? IsInsuranceAdded { get; set; }
        public InsurancePurchaseDetails InsurancePurchaseDetails { get; set; }
    }
}