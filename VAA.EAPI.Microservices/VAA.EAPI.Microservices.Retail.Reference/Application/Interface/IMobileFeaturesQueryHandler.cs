using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Interface
{
    public interface IMobileFeaturesQueryHandler
    {
        Task<MobileFeatures> HandleAsync(CancellationToken cancellationToken);
    }
}
