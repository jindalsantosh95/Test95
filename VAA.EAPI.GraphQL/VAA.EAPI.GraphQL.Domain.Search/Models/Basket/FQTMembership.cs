namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class FQTMembership
    {
        [GraphQLName("fqtProgramme")]
        public string? FQTProgramme { get; set; }
        [GraphQLName("fqtNumber")]
        public string? FQTNumber { get; set; }
        [GraphQLName("fqtValidityStatus")]
        public FQTValidationStatus? FQTValidityStatus { get; set; } = FQTValidationStatus.Unchecked;    // Optional because it is not required (passed) on input, but is required on output - Its a GraphQL thing, not C#
        public string? Validation { get; set; }
        [GraphQLName("fqtTierCode")]
        public string? FQTTierCode { get; set; }
        [GraphQLName("fqtPoints")]
        public string? FQTPoints { get; set; }
    }
}
