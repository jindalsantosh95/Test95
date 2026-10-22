using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Services.Analytics;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners
{
    public interface IMetaPartnerEventCommandHandler
    {
          Task<bool> Handle(MetaPartnerEventCommand request, CancellationToken cancellationToken);
    }
}
