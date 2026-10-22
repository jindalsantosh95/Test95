using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Security.Applications.Command.ShopsAway
{
    public class ShopsAwayTokenCommand : IRequest<Response<string>>
    {
        public string MemberDetails { get; set; }
    }
}
