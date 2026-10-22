namespace VAA.EAPI.GraphQL.Domain.Search.Infrastructure.Interfaces
{
    public interface IRefDataService<T>
    {

        /// <summary>
        /// Gets the dataset from ReferenceMS, throwing an exception if there is a failure
        /// </summary>
        /// <param name="filterValues">Restrict to items with one of these values</param>
        Task<IEnumerable<T>> Query(params string[] filterValues);

        /// <summary>
        /// Gets the dataset from ReferenceMS, returning an empty set if there is a failure
        /// </summary>
        /// <param name="filterValues">Restrict to items with one of these values</param>
        Task<IEnumerable<T>> TryQuery(params string[] filterValues);
    }
}
