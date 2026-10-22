using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Vred;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.VredMs
{
    public class UpdateVredCustomerDetailsResponseQuery: UpdateVredCustomerDetailsQueryResponse
    {
        public string CorrelationId { get; set; }        
    }
}
