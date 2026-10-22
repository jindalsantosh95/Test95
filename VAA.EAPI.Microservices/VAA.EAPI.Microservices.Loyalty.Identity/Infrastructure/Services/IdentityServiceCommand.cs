using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class IdentityServiceCommand : IRequest<bool>
    {
        public string Api { get; set; }
        public string Content { get; set; }
        public IdentityServiceCommand(string api, string content)
        {
            Api = api;
            Content = content;
        }

    }
}

