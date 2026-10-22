using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Exceptions.InflightCustomer.CustomException;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.InflightProfile;
using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class CustomerProfileRepoHandler : IRequestHandler<CustomerProfileRepoQuery, CustomerProfile>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerProfileRepoHandler> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public CustomerProfileRepoHandler(IMediator mediator, ILogger<CustomerProfileRepoHandler> logger, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
        }

        public async Task<CustomerProfile> Handle(CustomerProfileRepoQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Customer Profile Repo handler");
            var serviceResponse = await _mediator.Send(new RetrieveMemberQuery(request.CustomerId), cancellationToken);
            if (!serviceResponse.Key)
            {
                _logger.LogInformation($"Customer Profile Repo handler || Error in Service layer response. {Json.Serialize(serviceResponse.Value)}");
                throw new CustomerProfileException(ErrorCodes.CustomerProfileBusinessException);
            }
            CustomerProfile customerProfile = new CustomerProfile();
            if (serviceResponse.Value != null)
            {
                _logger.LogInformation($"Customer Profile Repo handler || Success response from Service layer.");
                Proxy.SCVPlusProfile.CustomerProfile retrieveMemberResponse = Json.Deserialize<Proxy.SCVPlusProfile.CustomerProfile>(serviceResponse.Value);
                var countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();

                customerProfile = MemberProfileResponseMapper.MapMemberDetailsToCustomerProfile(retrieveMemberResponse, request.CustomerId, countryDetails);
                if (customerProfile == null)
                {
                    _logger.LogInformation("Customer Profile Repo Handler || No data mapped to Inflight Customer profile");
                    throw new CustomerProfileException(ErrorCodes.CustomerProfileBusinessException);
                }
            }
            return customerProfile;
        }
    }
}
