using MediatR;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.FlightServiceMS;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Services.FlightServicesMS
{
    public class FlightServicesMSCommand : FlightServiceRequest, IRequest<FlightServiceResponse>
    {
    }
}
