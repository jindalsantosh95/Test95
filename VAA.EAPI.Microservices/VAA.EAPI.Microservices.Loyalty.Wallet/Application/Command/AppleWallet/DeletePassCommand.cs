using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Command.AppleWallet
{
    public class DeletePassCommand : IRequest<Response<int>>
    {
        public string DeviceId { get; set; }
        public string PassTypeId { get; set; }
        public string PassSerial { get; set; }
        public string PushToken { get; set; }
    }
}
