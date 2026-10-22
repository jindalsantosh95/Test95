using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.EntraFacade.Domain;

namespace VAA.Facade.EntraFacade.Application.Interfaces
{
    public interface ICustomAuthExtensionAdaptor
    {
        Task<Response<ResponseObject>> CustomToken(EntraRequest entraCode, CancellationToken cancellationToken);
        Task<Response<ResponseObject>> CustomEmailOtp(EntraRequest entraOtpequest, CancellationToken cancellationToken);
    }
}
