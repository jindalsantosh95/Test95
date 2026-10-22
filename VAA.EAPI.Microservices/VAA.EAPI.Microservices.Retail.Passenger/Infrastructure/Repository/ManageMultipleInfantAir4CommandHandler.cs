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
    public class ManageMultipleInfantAir4CommandHandler : IRequestHandler<ManageMultipleInfantAir4Command, ManageMultipleInfantV2Response>
    {
        private readonly IAir4PassengerV4 _iAir4PassengerV4;

        public ManageMultipleInfantAir4CommandHandler(IAir4PassengerV4 iAir4PassengerV4, ICacheConfig cacheConfig)
        {
            _iAir4PassengerV4 = iAir4PassengerV4 ?? throw new ArgumentNullException(nameof(iAir4PassengerV4));
        }

        public async Task<ManageMultipleInfantV2Response> Handle(ManageMultipleInfantAir4Command request, CancellationToken cancellationToken)
        {

            var manageMultipleInfantRequestType = ManageMultipleInfantReqResMapper.ManageMultipleInfantRequestTypeMapper(request);
            var manageMultipleInfantResponseType = await _iAir4PassengerV4.Air4ManageMultipleInfant(manageMultipleInfantRequestType);
            return ManageMultipleInfantReqResMapper.ManageMultipleInfantResponseTypeMapper(manageMultipleInfantResponseType);

        }
    }
}
