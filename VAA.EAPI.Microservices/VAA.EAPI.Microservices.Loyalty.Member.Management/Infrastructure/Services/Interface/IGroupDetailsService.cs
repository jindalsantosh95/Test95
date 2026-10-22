using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface
{
    public interface IGroupDetailsService
    {
        public Task<Result<Response<HouseholdDetailsResponse>>> GetHouseholdAccountDetails(string customerId,CancellationToken cancellationToken);
    }
}
