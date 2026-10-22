using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Application.Commands
{
    public class AddPassengerCommand : AddRequestType, IRequest<Result<Response<AddResponseType>>>
    {
        internal string ChannelId { get; set; }

    }
}
