using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.ValidateUniqueId;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Application.Commands.ValidateUniqueId
{
    public interface IValidateUniqueIdHandler
    {
        Task<Result<Response<ValidateUniqueIdResponse>>> Handle(ValidateUniqueIdCommand request, CancellationToken cancellationToken);
    }
}
