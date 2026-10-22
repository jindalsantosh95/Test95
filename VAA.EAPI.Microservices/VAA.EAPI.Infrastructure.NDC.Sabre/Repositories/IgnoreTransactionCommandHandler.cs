using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.IgnoreTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class IgnoreTransactionCommandHandler : IRequestHandler<IgnoreTransactionCommand, bool>
    {
        private readonly IIgnoreTransaction _ignoreTransaction;

        public IgnoreTransactionCommandHandler(IIgnoreTransaction ignoreTransaction)
        {
            _ignoreTransaction = ignoreTransaction ?? throw new ArgumentNullException(nameof(ignoreTransaction));
        }

        public async Task<bool> Handle(IgnoreTransactionCommand request, CancellationToken cancellationToken)
        {
            var IgnoreTransactionResponse = await _ignoreTransaction.IgnoreTransactionAsync(IgnoreTransactionRQMapper.IgnoreTransactionRequestMapper(request));

            return IgnoreTransactionRSMapper.IgnoreTransactionResponseMapper(IgnoreTransactionResponse);
        }
    }
}
