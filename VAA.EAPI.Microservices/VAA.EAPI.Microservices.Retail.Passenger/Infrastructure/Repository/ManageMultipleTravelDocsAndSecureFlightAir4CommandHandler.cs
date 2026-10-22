using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;
namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class ManageMultipleTravelDocsAndSecureFlightAir4CommandHandler : IRequestHandler<ManageMultipleTravelDocsAndSecureFlightAir4Command, ManageMultipleTravelDocsAndSecureFlightResponseType>
    {
        private readonly IAir4PassengerV4 _iAir4PassengerV4;

        public ManageMultipleTravelDocsAndSecureFlightAir4CommandHandler(IAir4PassengerV4 iAir4PassengerV4)
        {
            _iAir4PassengerV4 = iAir4PassengerV4 ?? throw new ArgumentNullException(nameof(iAir4PassengerV4));

        }

        public async Task<ManageMultipleTravelDocsAndSecureFlightResponseType> Handle(ManageMultipleTravelDocsAndSecureFlightAir4Command request, CancellationToken cancellationToken)
        {
            var addPassengerRequest = ManageMultipleTravelDocsAndSecureFlightReqRespMapper.ManageMultipleTravelDocsAndSecureFlightReq(request);
            var serviceResponse = await _iAir4PassengerV4.Air4ManageMultipleTravelDocsAndSecureFlight(addPassengerRequest);
            return ManageMultipleTravelDocsAndSecureFlightReqRespMapper.ManageMultipleTravelDocsAndSecureFlightRes(serviceResponse);
        }
    }
}
