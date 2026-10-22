

using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class AncillaryOrder
    {
        public OrderItemId OrderItemId { get; set; }
        public SupplierReference SupplierReference { get; set; }
        public OfferIdReference OfferIdReference { get; set; }
        public OfferItemIdReference OfferItemIdReference { get; set; }
        public IEnumerable<AncillaryOrderItem> AncillaryOrderItems { get; set; }
        public IEnumerable<FlightProduct> FlightProduct { get; set; }
        public ProductSpec ProductSpec { get; set; }
        public IEnumerable<ServiceFulfillment> ServiceFulfillments { get; set; }
        public IEnumerable<ServicePayment> ServicePayments { get; set; }
        public IEnumerable<ServiceStatus> ServiceStatus { get; set; }
        public ActualStartDeliveryDateTime ActualStartDeliveryDateTime { get; set; }
        public PlannedStartDeliveryDateTime PlannedStartDeliveryDateTime { get; set; }
        public ActualEndDeliveryDateTime ActualEndDeliveryDateTime { get; set; }
        public PlannedEndDeliveryDateTime PlannedEndDeliveryDateTime { get; set; }
        public string QuantityDelivered { get; set; }
        public IEnumerable<Tracking> Tracking { get; set; }
    }
}
