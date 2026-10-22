using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.IFGAgentFopLookUp;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.IFGAgentFopLookUp
{
    public class IFGAgentFopLookUpCommand : IFGAgentFopLookUpRequest , IRequest<IFGAgentFopLookUpResponse>
    {
    }
}
