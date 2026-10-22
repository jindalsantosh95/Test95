using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IHouseholdDetailsRetrievalQuery
    {
        Task<Result<Response<HouseholdDetailsResponse>>> Handle(HouseholdDetailsRetrievalQuery request, CancellationToken cancellationToken);
    }
}
