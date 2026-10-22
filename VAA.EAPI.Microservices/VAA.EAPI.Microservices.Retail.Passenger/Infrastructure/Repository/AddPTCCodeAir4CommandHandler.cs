using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Passenger.Infrastructure.Repository
{
    public class AddPTCCodeAir4CommandHandler : IRequestHandler<AddPTCCodeAir4Command, AddPTCCodesResponseType>
    {
        private readonly IAir4PassengerV4 _iAir4PassengerV4;
        private readonly ICacheConfig _cacheConfig;
        public AddPTCCodeAir4CommandHandler(IAir4PassengerV4 iAir4PassengerV4, ICacheConfig cacheConfig)
        {
            _iAir4PassengerV4 = iAir4PassengerV4 ?? throw new ArgumentNullException(nameof(iAir4PassengerV4));
            _cacheConfig = cacheConfig ?? throw new ArgumentNullException(nameof(cacheConfig));
        }

        public async Task<AddPTCCodesResponseType> Handle(AddPTCCodeAir4Command request, CancellationToken cancellationToken)
        {
            var addPTCCodeRequest = AddPTCCodeReqResMapper.AddPTCCodesRequestTypeMapper(request);
            var addPTCCodeServiceResponseType = await _iAir4PassengerV4.Air4AddPTC(addPTCCodeRequest);
            return AddPTCCodeReqResMapper.AddPTCCodesResponseTypeMapper(addPTCCodeServiceResponseType);

        }
    }
}
