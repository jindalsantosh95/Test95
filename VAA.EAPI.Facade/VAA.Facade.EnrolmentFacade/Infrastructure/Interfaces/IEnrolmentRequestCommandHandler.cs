using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Domain;
using VAA.Facade.EnrolmentFacade.Infrastructure.Entities;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces
{
    public interface IEnrolmentRequestCommandHandler
    {
        Task<Result<Response<EnrolmentResponse>>> Handle(EnrolmentRequestCommand request, CancellationToken cancellationToken);
    }
}
