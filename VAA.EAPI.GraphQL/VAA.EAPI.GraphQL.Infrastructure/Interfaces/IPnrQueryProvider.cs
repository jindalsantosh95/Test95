using System.Collections.Generic;
using System.Threading.Tasks;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IPnrQueryProvider
    {
        Task<IEnumerable<string>> RetrievePNRsbyEmail(string emailAddress);
    }
}
