using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IHouseHoldAccountLinkingCommand
    {
        Task<Result<Response<HouseHoldAccountLinkingResponse>>> Handle(HouseHoldAccountLinkingCommand houseHoldAccountLinkingRequest, CancellationToken cancellationToken);
    }
}
