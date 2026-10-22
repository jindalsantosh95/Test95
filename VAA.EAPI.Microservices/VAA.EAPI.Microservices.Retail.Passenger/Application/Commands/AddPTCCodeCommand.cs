using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Application.Commands
{
    public class AddPTCCodeCommand : AddPTCCodesRequestType, IRequest<Result<Response<AddPTCCodesResponseType>>>
    {
        public string ChannelId { get; set; }
    }
}
