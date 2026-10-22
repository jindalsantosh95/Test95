using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Interface
{
    public interface IBoardingPassWalletService
    {
        Task<Result<Response<WalletResponse>>> GenerateWalletLinksAsync(WalletPassRequest walletPassRequest);
    }
}
