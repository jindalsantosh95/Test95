using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Vred.Constants;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository
{
    public class VRedRepoHandler : IRequestHandler<VredRepoQuery, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VRedRepoHandler> _logger;

        public VRedRepoHandler(IMediator mediator, ILogger<VRedRepoHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(VredRepoQuery request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            #region VRed Member enrolment
            VRedEnrolmentResponse vredProfileResponse = await VRedEnrolment(request.CreateCustomerEntities);

            if (vredProfileResponse.ErrorResponse != null && !vredProfileResponse.ErrorResponse.Message.Contains("User already exists"))
            {
                _logger.LogInformation("LoyaltyProfileSyncResponse - Error Response");
                return false;
            }

            return true;
            #endregion
        }

        private async Task<VRedEnrolmentResponse> VRedEnrolment(CreateCustomerEntities createCustomerEntities)
        {
            if (createCustomerEntities == null) throw new ArgumentNullException(nameof(createCustomerEntities));

            var vRedRequest = VRedRequestMapper.MapToVRedRequest(createCustomerEntities);
            var enrolmentChannel = string.Empty;

            if (ConfigReader.Get<bool>(Application.VredEnableSignDate) && (createCustomerEntities.EnrollmentChannel == Application.WebMyaChannelID || createCustomerEntities.EnrollmentChannel == Application.AppMyaChannelID || createCustomerEntities.EnrollmentChannel == Application.PowerAppChannelId))
            {
                vRedRequest.MarketingPreferencesSignedAt = vRedRequest.TermsAndConditionsSignedAt = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);

                if (string.Equals(createCustomerEntities?.AddressDetails?.FirstOrDefault()?.CountryCode, Application.USCountryCode, StringComparison.OrdinalIgnoreCase))
                {
                    vRedRequest.State = createCustomerEntities?.AddressDetails?.FirstOrDefault()?.Region;
                }

                vRedRequest.MarketingAccepted = createCustomerEntities.MarketingAccepted;
                enrolmentChannel = createCustomerEntities.EnrollmentChannel;
            }

            return await _mediator.Send(new VredServiceQuery(vRedRequest, enrolmentChannel));
        }
    }
}
