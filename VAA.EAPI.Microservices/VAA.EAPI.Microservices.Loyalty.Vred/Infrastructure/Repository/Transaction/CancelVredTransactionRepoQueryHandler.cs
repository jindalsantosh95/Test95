using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Transaction
{
    public class CancelVredTransactionRepoQueryHandler : IRequestHandler<CancelVredTransactionRepoQuery, UpdateTransactionResponse>
    {
        private readonly IUpdateVredTransactionServiceHandler _updateVredTransactionServiceQueryHandler;

        public CancelVredTransactionRepoQueryHandler(IUpdateVredTransactionServiceHandler updateVredTransactionServiceQueryHandler)
        {
            _updateVredTransactionServiceQueryHandler = updateVredTransactionServiceQueryHandler ?? throw new ArgumentNullException(nameof(updateVredTransactionServiceQueryHandler));

        }

        public async Task<UpdateTransactionResponse> Handle(CancelVredTransactionRepoQuery req, CancellationToken cancellationToken)
        {
            return await _updateVredTransactionServiceQueryHandler.Handle(new UpdateVredTransactionService((UpdateVredTransactionRequest)req.CancelVredTransactionData), cancellationToken);
        }
    }
}
