namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class AccountableDocumentReference
    {
        public string? DocumentType { get; set; }
        public string? DocumentNumber { get; set; }
        public string? DocumentIssuer { get; set; }
        public string? IssueDate { get; set; }
    }
}
