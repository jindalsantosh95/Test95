using System;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Query.Interface;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities;
using VAA.EAPI.Microservices.CRM.CustomerProfile.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Application.Query
{
    public class AmperityHandler : IAmperityHandler
    {
        private readonly IAmperityService _amerityService;

        public AmperityHandler(IAmperityService amerityService)
        {
            _amerityService = amerityService?? throw new ArgumentNullException(nameof(amerityService));
        }

        public async Task<Response<AmperityDetails>> GetAmperityDetails(string loyaltyId)
        {
            return new Response<AmperityDetails>
            {
                Status = Status.OK,
                Value = (AmperityDetails)await _amerityService.GetAmperityDetails(loyaltyId)
            };
        }
    }
}
