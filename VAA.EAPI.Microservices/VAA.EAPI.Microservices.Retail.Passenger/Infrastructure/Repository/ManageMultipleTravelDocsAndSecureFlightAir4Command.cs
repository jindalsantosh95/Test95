using MediatR;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class ManageMultipleTravelDocsAndSecureFlightAir4Command : ManageMultipleTravelDocsAndSecureFlightRequestType, IRequest<ManageMultipleTravelDocsAndSecureFlightResponseType>
    {
        internal string ChannelId { get; set; }
    }
}
