using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IVredCardDetailsQueryProvider
    {
        Task<VredCardDetails> GetVredCardDetails(string memberId);

        Task<VRedResponse> VredPointBalance(string memberNumber);
        Task<VredStatementResponse> GetVredStatements(string memberNumber);
    }
}
