using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common.Serialisation;
using VAA.Facade.AzureB2C.Domain.Entities;
using VAA.Facade.AzureB2C.Domain.Model;

namespace VAA.Facade.AzureB2C.Application.Command
{
    public class B2CRequestCommand : IRequest<Result<Response<B2CResponse>>>
    {
        public B2CRequest B2CRequest { get; set; }

        public B2CRequestCommand(string b2cRequest)
        {
            B2CRequest = Json.DeserializeObject<B2CRequest>(b2cRequest);
        }
    }
}
