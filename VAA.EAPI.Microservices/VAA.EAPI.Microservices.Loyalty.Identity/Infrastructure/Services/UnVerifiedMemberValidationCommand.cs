
using MediatR;
using VAA.EAPI.Domain.Models.B2CGraph;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class UnVerifiedMemberValidationCommand : IRequest<B2CGraphResponse>
    {
        public string Api { get; set; }
        public string Query { get; set; }

        public UnVerifiedMemberValidationCommand(string api, string query)
        {
            Api = api;
            Query = query;
        }

    }
}

