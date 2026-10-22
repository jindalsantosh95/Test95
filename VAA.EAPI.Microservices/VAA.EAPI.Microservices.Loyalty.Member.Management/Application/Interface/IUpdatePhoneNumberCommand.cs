using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.CustomerProfile;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IUpdatePhoneNumberCommand
    {
        Task<Result<Response<string>>> Handle(UpdatePhoneNumberNotificationEventRequest updatePhoneNumberNotificationEventRequest, CancellationToken cancellationToken);
    }
}
