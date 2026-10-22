namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class PendingDocument
    {
        public string Format { get; set; }
        public string DocumentType { get; set; }
        public string DispatchMethods { get; set; }
        public string Template { get; set; }
        public IEnumerable<PendingDocumentAttribute> Attributes { get; set; }
        public DateTime? DispatchDateTime { get; set; }
        public string Description { get; set; }
    }
}
