using MediatR;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class PatchUserServiceCommand : IRequest<B2CUserResponse>
    {
        public string Api { get; set; }
        public string ObjectId { get; set; }
        public string Content { get; set; }
        public PatchUserServiceCommand(string api, string content, string objectId)
        {
            Api = api;
            Content = content;
            ObjectId = objectId;
        }
    }
}
