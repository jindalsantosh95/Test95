using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ConfigMS.Location;

namespace VAA.EAPI.NDC.Order.Infrastructure.Interface
{
    public interface ICityCodeQuery
    {
        Task<CityCodeData> Handle(string countryCode, CancellationToken cancellationToken);
    }
}
