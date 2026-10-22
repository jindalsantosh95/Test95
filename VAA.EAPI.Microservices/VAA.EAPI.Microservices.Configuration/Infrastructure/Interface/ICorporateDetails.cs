using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;

namespace VAA.EAPI.Microservices.Configuration.Infrastructure.Interface
{
    public interface ICorporateDetails
    {
        Task<CorporateData> Handle(string cbaClid, CancellationToken cancellationToken);
    }
}