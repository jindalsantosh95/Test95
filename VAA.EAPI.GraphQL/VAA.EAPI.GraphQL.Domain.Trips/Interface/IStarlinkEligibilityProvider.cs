namespace VAA.EAPI.GraphQL.Domain.Trips.Interface;

public interface IStarlinkEligibilityProvider
{
    Task<IReadOnlyDictionary<string, bool?>> GetEligibilityAsync(IEnumerable<string> tailNumbers);
}
