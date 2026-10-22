using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Interfaces
{
    public interface IRefDataService<T>
    {
        Task<IEnumerable<T>> Query(params string[] filterValues);

        Task<IEnumerable<R>> QueryAs<R>(params string[] filterValues);
    }
}
