using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Application.Commands
{
    public class AddPhoneCommand : AddPhoneRequestType, IRequest<Result<Response<AddPhoneResponseType>>>
    {
        public string ChannelId { get; set; }
    }
}
