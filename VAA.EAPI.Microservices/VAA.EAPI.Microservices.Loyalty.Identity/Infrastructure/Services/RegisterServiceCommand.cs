using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class RegisterServiceCommand : IRequest<B2CUserResponse>
    {
        public string Api { get; set; }
        public string Content { get; set; }
        public RegisterServiceCommand(string api, string content)
        {
            Api = api;
            Content = content;
        }
    }
}
