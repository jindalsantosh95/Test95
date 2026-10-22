namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class Service
    {
        public string? ServiceId { get; set; }
        public ProductOrder? ProductOrder { get; set; }
        public ProductSegmentAssociation? ProductSegmentAssociation { get; set; }
        public string? CurrentSeatAssignment { get; set; }
        public List<string>? ServiceBeneficiaries { get; set; }
        public List<string>? OrderInstructionRefs { get; set; }
        public List<PriceDetail>? PriceDetails { get; set; }
        public ServiceStatus? ServiceStatus { get; set; }
    }
}