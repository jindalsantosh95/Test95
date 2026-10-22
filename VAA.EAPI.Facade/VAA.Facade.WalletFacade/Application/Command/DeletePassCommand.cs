using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.Facade.WalletFacade.Application.Command
{
    public class DeletePassCommand : IRequest<Response<int>>
    {
        public string DeviceId { get; set; }
        public string PassTypeId { get; set; }
        public string PassSerial { get; set; }       
    }
}
