using MediatR;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class AddPassengerAir4Command : AddRequestType, IRequest<AddResponseType>
    {
        internal string ChannelId { get; set; }
    }
}
