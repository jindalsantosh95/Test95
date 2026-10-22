using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Exceptions.LoyaltyProfile.CustomException;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.LoyaltyProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.LoyaltyProfile;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.LoyaltyProfile
{
    public class LoyaltyProfileRepoHandler : IRequestHandler<LoyaltyProfileRepoQuery, LoyaltyProfileDetails>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LoyaltyProfileRepoHandler> _logger;

        public LoyaltyProfileRepoHandler(IMediator mediator, ILogger<LoyaltyProfileRepoHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<LoyaltyProfileDetails> Handle(LoyaltyProfileRepoQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Loyalty Profile Repo handler");
            var serviceResponse = await _mediator.Send(new RetrieveLoyaltyMemberQuery(request.MemberNumber), cancellationToken);
            if (!serviceResponse.Key)
            {
                _logger.LogInformation($"Loyalty Profile Repo handler || Error in Service layer response. {Json.Serialize(serviceResponse.Value)}");
                throw new LoyaltyProfileException(ErrorCodes.CustomerProfileBusinessException);
            }
            LoyaltyProfileDetails loyaltyProfileDetails = new();
            if (serviceResponse.Value != null)
            {
                _logger.LogInformation($"Loyalty Profile Repo handler || Success response from Service layer.");
                loyaltyProfileDetails = Json.Deserialize<LoyaltyProfileDetails>(serviceResponse.Value);
                if (loyaltyProfileDetails == null)
                {
                    _logger.LogInformation("Loyalty Profile Repo Handler || No data mapped to Loyalty Customer profile");
                }
            }
            return loyaltyProfileDetails;
        }
    }
}
