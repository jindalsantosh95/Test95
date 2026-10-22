using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public interface ICityCodeQuery
    {
        Task<CityCodeData> Handle(string countryCode, CancellationToken cancellationToken);
    }
}