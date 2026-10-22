using MediatR;
using VAA.EAPI.Domain.Models.B2CGraph;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class UpdateB2CServiceCommand : IRequest<bool>
    {
        public B2CGraphResponse EmailB2CUpdateData { get; set; }

        public UpdateB2CServiceCommand(B2CGraphResponse updateEmailDocument)
        {
            EmailB2CUpdateData = updateEmailDocument;
        }
    }
}
