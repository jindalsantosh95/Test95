using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository
{
    public interface IMembershipChangeRepository
    {
        public Task<Result<Response<TierInfo>>> AddMembershipChangeDetails(CreateCustomerEntities createCustomerEntities);
        public Task<Result<Response<TierInfo>>> UpdateMembershipChangeDetails(CreateCustomerEntities createCustomerEntities, MembershipChange membershipChange);
    }
}
