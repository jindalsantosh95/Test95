namespace VAA.EAPI.GraphQL.Domain.Security.Models.NDC
{
    public class NDCAgencyDetails
    {
        [GraphQLDescription("Agency Id")]
        public string AgencyId { get; set; } = null!;

        [GraphQLDescription("Agency Name")]
        public string? AgencyName { get; set; } = null!;
    }
}
