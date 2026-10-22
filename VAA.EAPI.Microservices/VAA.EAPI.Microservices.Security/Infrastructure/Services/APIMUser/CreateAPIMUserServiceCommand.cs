using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMUserRequest;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMUser
{
    public class CreateAPIMUserServiceCommand : IRequest<KeyValuePair<bool, string>>
    {
        public CreateAPIMUserServiceCommand(UserCreationRequest request, string uri)
        {
            Request = request;
            Uri = uri;
        }

        public UserCreationRequest Request { get; set; }
        public string Uri { get; set; }
    }
}
