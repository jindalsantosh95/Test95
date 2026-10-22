using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class AddPhoneAir4CommandHandler : IRequestHandler<AddPhoneAir4Command, AddPhoneResponseType>
    {

        private readonly IAir4PassengerV4 _iAir4PassengerV4;

        public AddPhoneAir4CommandHandler(IAir4PassengerV4 iAir4PassengerV4)
        {
            _iAir4PassengerV4 = iAir4PassengerV4 ?? throw new ArgumentNullException(nameof(iAir4PassengerV4));

        }

        public async Task<AddPhoneResponseType> Handle(AddPhoneAir4Command request, CancellationToken cancellationToken)
        {

            var addPhoneRequest = AddPhoneV4AddReqResMapper.AddPhoneV4AddReq(request);
            var serviceResponse = await _iAir4PassengerV4.Air4AddPhone(addPhoneRequest);
            return AddPhoneV4AddReqResMapper.AddPhoneV4AddRes(serviceResponse.AddPhoneResponse1);

        }
    }
}
