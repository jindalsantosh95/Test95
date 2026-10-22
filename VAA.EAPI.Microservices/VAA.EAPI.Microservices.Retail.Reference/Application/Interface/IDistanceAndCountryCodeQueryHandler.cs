using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Reference.Application.Query;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Interface
{
    public interface IDistanceAndCountryCodeQueryHandler
    {
        Task<Result<Response<DistanceAndCountryCodeResponse>>> Handle(DistanceAndCountryCodeQuery request, CancellationToken cancellationToken);
    }
}
