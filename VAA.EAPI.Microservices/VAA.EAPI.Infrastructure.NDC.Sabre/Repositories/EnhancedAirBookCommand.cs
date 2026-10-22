using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirBook;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class EnhancedAirBookCommand : IRequest<EnhancedAirBookRQResponse>
    {
        public EnhancedAirBookRQRequest EnhancedAirBookRequest { get; set; }

    }
}
