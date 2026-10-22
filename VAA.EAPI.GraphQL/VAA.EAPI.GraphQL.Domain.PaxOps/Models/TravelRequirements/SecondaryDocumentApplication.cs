using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements
{
    public class SecondaryDocumentApplication
    {
        [GraphQLDescription("Identifier for the secondary-document link represented by this application option.")]
        public string SecondaryDocumentId { get; set; } = string.Empty;

        [GraphQLDescription("Source of this application option.")]
        public ApplicationType ApplicationType { get; set; }

        [GraphQLDescription("Customer-facing title for this application option.")]
        public string Title { get; set; } = string.Empty;

        [GraphQLDescription("Customer-facing guidance for this application option.")]
        public string Message { get; set; } = string.Empty;

        [GraphQLDescription("Approved application URL for this option, or null when no approved URL is available.")]
        public string? Url { get; set; }

        [GraphQLDescription("Customer-facing label describing the next application step.")]
        public string? UrlText { get; set; }

        public static explicit operator SecondaryDocumentApplication(Infrastructure.Models.CheckInMS.SecondaryDocumentApplication source) => new()
        {
            SecondaryDocumentId = source.SecondaryDocumentId,
            ApplicationType = source.ApplicationType,
            Title = source.Title,
            Message = source.Message,
            Url = source.Url,
            UrlText = source.UrlText
        };
    }
}
