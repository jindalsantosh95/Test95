using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class EndTransactionCommandHandler(IEndTransaction endTransaction) : IEndTransactionCommand
    {
        private readonly IEndTransaction _endTransaction = endTransaction ?? throw new ArgumentNullException(nameof(endTransaction));

        public async Task<EndTransactionRS> Handle(EndTransactionRQRequest request, CancellationToken cancellationToken)
        {
            return await _endTransaction.EndTransactionRQAsync(request);
        }
    }
}
