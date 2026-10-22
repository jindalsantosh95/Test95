using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Validation;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class VerifyMemberCommandHandler : IRequestHandler<VerifyMemberCommand, Response<string>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VerifyMemberCommandHandler> _logger;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public VerifyMemberCommandHandler(
            IMediator mediator,
            ILogger<VerifyMemberCommandHandler> logger,
            ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Response<string>> Handle(VerifyMemberCommand request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = "VerifyMember"
            };

            try
            {
                bool IsValidEmailId = ValidateEmail.Validate(request.MemberLoginRequest.EmailAddress);
                if (!IsValidEmailId)
                    return GetErrorResponse(Constants.ErrorCodes.InvalidEmailFormat);

                bool verifyEmailExists = await _mediator.Send(new VerifyMemberRepoCommand(request.MemberLoginRequest), cancellationToken);

                if (verifyEmailExists)
                    return GetErrorResponse(Constants.ErrorCodes.EmailExistsInB2C);

                trackAnalyticsEventCommand.Track("Verified", verifyEmailExists);

                return new Response<string>()
                {
                    Status = Status.OK,
                    Value = "Email Verified in B2C",
                    Errors = null
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error occured while verifying email address");
                throw;
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(trackAnalyticsEventCommand, cancellationToken);
            }
        }

        private static Response<string> GetErrorResponse(string errorCode)
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };

        }
    }
}
