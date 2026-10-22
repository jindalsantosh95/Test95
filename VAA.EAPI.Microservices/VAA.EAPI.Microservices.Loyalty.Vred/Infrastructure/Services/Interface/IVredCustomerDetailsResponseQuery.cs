
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.CustomerDetail;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface
{
    public interface IVredCustomerDetailsResponseQuery
    {
        Task<Response<UpdateVredCustomerDetailsQueryResponse>> Handle(UpdateVredCustomerDetailsResponseQuery request, CancellationToken cancellationToken);
    }
}
