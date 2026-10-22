using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class SecondaryDocumentRequirement
    {
        public DocumentType DocumentType { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryCode3Letter { get; set; }
        public string? CountryName { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public SecondaryDocumentStatus Status { get; set; }
        public List<SecondaryDocumentApplication> SecondaryDocumentApplicationOptions { get; set; } = [];
    }
}
