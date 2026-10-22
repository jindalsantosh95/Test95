using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface ILoyaltyMemberManagementProvider
    {
        Task<StatementTextResponse> GenerateStatementText(StatementTextrequest statementTextrequest);
    }
}
