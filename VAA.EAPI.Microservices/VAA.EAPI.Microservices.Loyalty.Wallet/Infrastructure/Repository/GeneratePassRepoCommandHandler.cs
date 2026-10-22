using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Wallet.Helper;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Repository
{
    public class GeneratePassRepoCommandHandler : IRequestHandler<GeneratePassRepoCommand, byte[]>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        public GeneratePassRepoCommandHandler(IOAuthTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

        public async Task<byte[]> Handle(GeneratePassRepoCommand request, CancellationToken cancellationToken)
        {
            return await GeneratePassService.GeneratePass(await _tokenProvider.AcquireTokenAsync(ConfigReader.Get(Constants.Environment.WalletFacadeAppId)), request.MemberName, request.FcNumber, request.TierCode, request.ValidUntil);
        }
    }
}
