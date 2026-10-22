using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners
{
    public interface IMetaPartnerCommandHandler
    {
        Task Handle(MetaPartnerCommand request, CancellationToken cancellationToken);
    }
}
