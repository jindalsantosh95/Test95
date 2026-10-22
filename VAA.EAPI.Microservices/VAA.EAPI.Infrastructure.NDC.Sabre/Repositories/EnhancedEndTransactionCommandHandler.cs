using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class EnhancedEndTransactionCommandHandler : IRequestHandler<EnhancedEndTransactionCommand, EnhancedEndTransactionRS>
    {
        private readonly IEnhancedEndTransaction _enhancedEndTransaction;

        public EnhancedEndTransactionCommandHandler(IEnhancedEndTransaction enhancedEndTransaction)
        {
            _enhancedEndTransaction = enhancedEndTransaction ?? throw new ArgumentNullException(nameof(enhancedEndTransaction));
        }

        public async Task<EnhancedEndTransactionRS> Handle(EnhancedEndTransactionCommand request, CancellationToken cancellationToken)
        {
            return await _enhancedEndTransaction.EnhancedEndTransactionRQAsync(request.EnhancedEndTransactionRQ);
        }
    }
}
