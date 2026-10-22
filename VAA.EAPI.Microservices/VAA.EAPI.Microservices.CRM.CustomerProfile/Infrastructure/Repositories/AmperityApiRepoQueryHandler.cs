using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Repositories
{
    public class AmperityApiRepoQueryHandler : IAmperityApiRepoQuery
    {
        private readonly ILogger<AmperityApiRepoQueryHandler> _logger;

        public AmperityApiRepoQueryHandler(ILogger<AmperityApiRepoQueryHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<Passenger>> Handle(CustomerProfileRequest customerProfileRequest, CancellationToken cancellationToken)
        {
            //get amperity Id
            customerProfileRequest.Passenger.AmperityId = Guid.NewGuid().ToString();
            
            if (!string.IsNullOrEmpty(customerProfileRequest.Passenger.AmperityId))
                return new Response<Passenger>() { Status = Status.OK, Value = customerProfileRequest.Passenger };
            
            else return new Response<Passenger>()
            {
                Status = Status.BusinessException
            };

        }
    }
}
