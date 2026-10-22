using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class AncillaryOrder
    {
        public OrderItemId OrderItemId { get; set; }
        public SupplierReference SupplierReference { get; set; }
        public OfferIdReference OfferIdReference { get; set; }
        public OfferItemIdReference OfferItemIdReference { get; set; }
        public List<AncillaryOrderItem> AncillaryOrderItems { get; set; }
        public List<FlightProduct> FlightProduct { get; set; }
        public ProductSpec ProductSpec { get; set; }
        public List<ServiceFulfillment> ServiceFulfillments { get; set; }
        public List<ServicePayment> ServicePayments { get; set; }
        public List<ServiceStatus> ServiceStatus { get; set; }
        public ActualStartDeliveryDateTime ActualStartDeliveryDateTime { get; set; }
        public PlannedStartDeliveryDateTime PlannedStartDeliveryDateTime { get; set; }
        public ActualEndDeliveryDateTime ActualEndDeliveryDateTime { get; set; }
        public PlannedEndDeliveryDateTime PlannedEndDeliveryDateTime { get; set; }
        public string QuantityDelivered { get; set; }
        public List<Tracking> Tracking { get; set; }
    }
}
