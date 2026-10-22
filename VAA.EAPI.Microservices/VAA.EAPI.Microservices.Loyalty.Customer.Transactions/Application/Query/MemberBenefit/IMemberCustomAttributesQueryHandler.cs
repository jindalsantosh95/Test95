using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MemberBenefit
{
    public interface IMemberCustomAttributesQueryHandler
    {
        Task<Result<Response<MemberCustomAttributesResponse>>> Handle(MemberCustomAttributesQuery request, CancellationToken cancellationToken);
    }
}
