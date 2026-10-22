using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.Interfaces
{
    public interface ISqlQueryService
    {
        Task<IEnumerable<T>> QueryAsync<T>(string connectionString, string query, object parameters, CommandType commandType, CancellationToken cancellationToken);
    }
}
