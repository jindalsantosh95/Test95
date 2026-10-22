using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces
{
    public interface IVredMutationProvider
    {
        Task<VRedTransactionResponse> PostTransactionToVred(VredSyncTransactionRequest vredTransactionRequest);
        Task<VRedTransactionResponse> CancelTransactionVred(VredCancelTransactionRequest vredCancelRequest);
    }
}