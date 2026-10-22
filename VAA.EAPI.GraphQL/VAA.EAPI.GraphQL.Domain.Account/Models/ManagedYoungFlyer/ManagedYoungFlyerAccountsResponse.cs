namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer
{
    public sealed class ManagedYoungFlyerAccountsResponse
    {
        public string ParentCustomerId { get; init; }
        public string ParentFcNumber { get; init; }
        public IEnumerable<ManagedYoungFlyerAccount> YoungFlyers { get; init; } = Array.Empty<ManagedYoungFlyerAccount>();
    }
}
