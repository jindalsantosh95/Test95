using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IContactNoteQueryProvider
    {
        Task<IEnumerable<ContactNote>> Query(string customerId);
    }
}
