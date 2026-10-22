using HotChocolate;
using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;
using VAA.EAPI.GraphQL.Domain.PaxOps.Models.Enums;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements
{
    public class SecondaryDocumentRequirement
    {
        [GraphQLDescription("Backend-driven secondary-document type.")]
        public DocumentType DocumentType { get; set; }

        [GraphQLDescription("ISO 3166-1 alpha-2 country code associated with this secondary-document requirement, for example GB or US.")]
        public string? CountryCode { get; set; }

        [GraphQLDescription("ISO 3166-1 alpha-3 country code associated with this secondary-document requirement, for example GBR or USA.")]
        public string? CountryCode3Letter { get; set; }

        [GraphQLDescription("Country name associated with this secondary-document requirement.")]
        public string? CountryName { get; set; }

        [GraphQLDescription("Raw secondary-document name supplied by TRM.")]
        public string? Name { get; set; }

        [GraphQLDescription("Customer-facing document name supplied or derived by the backend.")]
        public string? DisplayName { get; set; }

        [GraphQLDescription("Completion state for this individual secondary-document requirement.")]
        public SecondaryDocumentStatus Status { get; set; }

        [GraphQLDescription("Application options supplied for this requirement, tagged by vendor or government source.")]
        public List<SecondaryDocumentApplication> SecondaryDocumentApplicationOptions { get; set; } = [];

        public static explicit operator SecondaryDocumentRequirement(Infrastructure.Models.CheckInMS.SecondaryDocumentRequirement source) => new()
        {
            DocumentType = source.DocumentType,
            CountryCode = source.CountryCode,
            CountryCode3Letter = source.CountryCode3Letter,
            CountryName = source.CountryName,
            Name = source.Name,
            DisplayName = source.DisplayName,
            Status = source.Status,
            SecondaryDocumentApplicationOptions = source.SecondaryDocumentApplicationOptions?.Select(application => (SecondaryDocumentApplication)application).ToList() ?? []
        };
    }
}
