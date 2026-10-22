namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class Document
    {
        public string LeadPassengerName { get; set; }
        public DateOnly? BookingDate { get; set; }
        public DateOnly? DepartureDate { get; set; }
        public int DocumentId { get; set; }
        public int DocumentStoreId { get; set; }
        public string DocumentType { get; set; }
        public IEnumerable<DocumentGroup> DocumentGroups { get; set; }
        public string DocumentName { get; set; }
        public string Description { get; set; }
        public string DispatchStatus { get; set; }
        public string DispatchMethods { get; set; }
        public string DocumentCategory { get; set; }
        public string DocumentSubCategory { get; set; }
        public bool QualityChecked { get; set; }
        public bool DeletedFromStorage { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public IEnumerable<DispatchDetail> DispatchDetails { get; set; }
        public string DispatchSummary { get; set; }
        public bool QualityCheckedInfo { get; set; }
        public int BookingId { get; set; }
        public string BookingStatus { get; set; }
        public string OptionStatus { get; set; }
        public int? ItemNumber { get; set; }
        public string ProductCode { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public Supplier Supplier { get; set; }
    }
}
