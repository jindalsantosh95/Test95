using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler : IRequestHandler<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery, Customer>
    {
        private readonly IAir4PassengerV4 _iAir4PassengerV4;
        private readonly ILogger<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler> _logger;

        public PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler(IAir4PassengerV4 iAir4PassengerV4, ILogger<PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoHandler> logger)
        {
            _iAir4PassengerV4 = iAir4PassengerV4 ?? throw new ArgumentNullException(nameof(iAir4PassengerV4));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Customer> Handle(PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery handler-Start");
                var addPassengerRequest = PassengerV4ManageMultipleTravelDocsAndSecureFlightRQMapper.ManageMultipleTravelDocsAndSecureFlightReq(request.Customer, request.SessionId, request.RetrieveFlag);
                var response = await _iAir4PassengerV4.Air4ManageMultipleTravelDocsAndSecureFlight(addPassengerRequest);
                return PassengerV4ManageMultipleTravelDocsAndSecureFlightRSMapper.MapToPassengerV4Response(request.Customer, response, request.RetrieveFlag);
            }
            finally
            {
                _logger.LogInformation($"PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery handler-End");
            }
        }
    }
}
