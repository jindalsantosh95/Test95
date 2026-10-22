using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class SecondaryDocumentApplication
    {
        public string SecondaryDocumentId { get; set; } = string.Empty;
        public ApplicationType ApplicationType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string? UrlText { get; set; }
    }
}
