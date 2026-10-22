namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IManagedYoungFlyerRelationshipProvider
    {
        Task<IReadOnlyCollection<string>> GetManagedYoungFlyersCustomerIdsAsync(string parentCustomerId);
    }
}
