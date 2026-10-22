using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Repositories
{
    public interface IAmperityApiRepoQuery  
    {
        Task<Response<Passenger>> Handle(CustomerProfileRequest customerProfileRequest, CancellationToken cancellationToken);        
    }
}
