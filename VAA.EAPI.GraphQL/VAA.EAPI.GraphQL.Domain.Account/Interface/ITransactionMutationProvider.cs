using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.VRedMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface ITransactionMutationProvider
    {
        Task<VredTransactionResponse> CreateVredFlightTransaction(CreateTransactionRequest vredFlightTransactionRequest);
        Task<VredTransactionResponse> CreateEarnTransaction(CreateTransactionRequest earnTransactionRequest);
        Task<VredTransactionResponse> CreateBurnTransaction(CreateTransactionRequest burnTransactionRequest);
        Task<VredTransactionResponse> CancelTransaction(CancellationRequest cancelTransactionRequest);
    }
}
