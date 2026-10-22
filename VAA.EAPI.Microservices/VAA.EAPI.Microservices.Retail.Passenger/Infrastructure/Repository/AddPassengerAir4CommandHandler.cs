using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class AddPassengerAir4CommandHandler : IRequestHandler<AddPassengerAir4Command, AddResponseType>
    {
        private readonly IAir4PassengerV4 _iAir4PassengerV4;
        public AddPassengerAir4CommandHandler(IAir4PassengerV4 iAir4PassengerV4)
        {
            _iAir4PassengerV4 = iAir4PassengerV4 ?? throw new ArgumentNullException(nameof(iAir4PassengerV4));

        }

        public async Task<AddResponseType> Handle(AddPassengerAir4Command request, CancellationToken cancellationToken)
        {

            var addPassengerRequest = PassengerV4AddReqResMapper.PassengerV4AddReq(request);
            var serviceResponse = await _iAir4PassengerV4.Air4AddPassenger(addPassengerRequest);
            return PassengerV4AddReqResMapper.PassengerV4AddRes(serviceResponse);
        }
    }
}
