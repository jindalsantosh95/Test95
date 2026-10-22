namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer
{
    public sealed class HouseholdMember
    {
        public string CustomerId { get; set; }
        public string RelationshipType { get; set; }
        public DateTime? AssociationDate { get; set; }
    }
}
