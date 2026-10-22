using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Service
    {
        public string ServiceId { get; set; }
        public FlightProduct FlightProductOrder { get; set; }
        public ProductSpecification ProductOrder { get; set; }
        public int? QuantityRequired { get; set; }
        public SegmentProduct ProductSegmentAssociation { get; set; }
        public IEnumerable<ProductInstanceAssignment> ProductInstanceAssignments { get; set; }
        public ProductRelationship ProductAssociation { get; set; }
        public Customer ServiceBeneficiary { get; set; }
        public PriceDetail ServicePrice { get; set; }
        public ItemStatus ServiceStatus { get; set; }


    }
}
