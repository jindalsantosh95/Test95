using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.IFGAgentFopLookUp;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Command
{
    public interface IIFGAgentFopLookUp
    {
        Task<Result<Response<IFGAgentFopLookUpResponse>>> Handle(IFGAgentFopLookUpRequest iFGAgentFopLookUpRequest, System.Threading.CancellationToken cancellationToken);
    }
}