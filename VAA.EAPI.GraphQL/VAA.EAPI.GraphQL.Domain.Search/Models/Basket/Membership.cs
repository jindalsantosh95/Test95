namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public sealed class Membership
    {
        public string MembershipNumber { get; set; }
        public string MemberSurname { get; set; }
        public string Link { get; set; }
        public string Validation { get; set; }
        public string ValidateLink { get; set; }
    }
}