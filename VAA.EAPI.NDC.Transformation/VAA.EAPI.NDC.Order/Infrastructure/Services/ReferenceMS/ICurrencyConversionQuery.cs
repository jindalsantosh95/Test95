using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ReferenceMS
{
    public interface ICurrencyConversionQuery
    {
        Task<string> Handle(CancellationToken cancellationToken);
    }
}
