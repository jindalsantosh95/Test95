using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Vred;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.VredMs
{
    public interface IVredCustomerDetailsResponseQuery
    {
        Task<UpdateVredCustomerDetailsRS> Handle(UpdateVredCustomerDetailsResponseQuery request, CancellationToken cancellationToken);
    }
}