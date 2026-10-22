using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IAutoAccrualReferenceValidationService
    {
        Task ValidateAsync(AutoAccrualRequest request, CancellationToken cancellationToken);
    }
}
