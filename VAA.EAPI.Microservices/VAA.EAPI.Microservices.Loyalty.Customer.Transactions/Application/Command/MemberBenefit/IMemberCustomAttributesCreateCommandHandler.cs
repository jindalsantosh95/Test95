using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.MemberBenefit
{
    public interface IMemberCustomAttributesCreateCommandHandler
    {
        Task<Result<Response<MemberCustomAttributesCommandResponse>>> Handle(MemberCustomAttributesCreateCommand request, CancellationToken cancellationToken);
    }
}
