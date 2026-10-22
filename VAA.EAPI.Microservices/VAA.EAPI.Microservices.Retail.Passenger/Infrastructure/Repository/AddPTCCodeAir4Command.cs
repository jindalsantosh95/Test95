using MediatR;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class AddPTCCodeAir4Command : AddPTCCodesRequestType, IRequest<AddPTCCodesResponseType>
    {
        public string ChannelId { get; set; }
    }
}
