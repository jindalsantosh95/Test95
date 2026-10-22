namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public sealed class FlyingClubProfile
    {
        public string ReasonForTravel { get; set; }
        public string Brand { get; set; }
        public IEnumerable<FlyingClubPreference> Preferences { get; set; }
    }
}
