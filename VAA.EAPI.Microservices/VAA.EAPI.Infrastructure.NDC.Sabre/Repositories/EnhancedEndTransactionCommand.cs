using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class EnhancedEndTransactionCommand : IRequest<EnhancedEndTransactionRS>
    {
        public EnhancedEndTransactionRQRequest EnhancedEndTransactionRQ { get; set; }
    }
}