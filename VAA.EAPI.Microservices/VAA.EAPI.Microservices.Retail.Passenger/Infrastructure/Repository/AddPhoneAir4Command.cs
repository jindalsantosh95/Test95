using MediatR;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class AddPhoneAir4Command : AddPhoneRequestType, IRequest<AddPhoneResponseType>
    {
        public string ChannelId { get; set; }
    }
}
