using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.TransactionStatement;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface
{
    public interface ITxnStatementTextService
    {
        public Task<StatementTextResponse> GetTransactionStatementText(TransactionStatementTextQuery request);
    }
}
