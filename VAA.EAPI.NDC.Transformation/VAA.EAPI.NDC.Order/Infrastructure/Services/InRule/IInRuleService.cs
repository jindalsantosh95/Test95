using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.InRule;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.InRule
{
    public interface IInRuleService
    {
        Task<DisruptionResponse> GetDisruptionResponseAsync(DisruptionRequest request, CancellationToken cancellationToken);
    }
}
