using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;

namespace VAA.EAPI.GraphQL.Domain.Search.Interfaces
{
    public interface ICustomerTransactionProvider
    {
        Task<IEnumerable<VoucherDetail>> RetrieveVoucher(string customerId);
    }
}
