namespace VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Interfaces
{
    public interface IRefDataService<T>
    {
        Task<IEnumerable<T>> Query(params string[] filterValues);
    }
}
