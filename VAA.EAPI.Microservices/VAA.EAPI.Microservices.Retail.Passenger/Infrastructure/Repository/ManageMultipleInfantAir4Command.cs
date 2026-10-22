using MediatR;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class ManageMultipleInfantAir4Command : ManageMultipleInfantV2RequestType, IRequest<ManageMultipleInfantV2Response>
    {
        public string ChannelId { get; set; }
    }
}
