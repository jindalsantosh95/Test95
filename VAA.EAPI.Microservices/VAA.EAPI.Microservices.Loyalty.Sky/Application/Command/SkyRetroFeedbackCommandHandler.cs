using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Sky.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Application.Command
{
    public class SkyRetroFeedbackCommandHandler : IRequestHandler<SkyRetroFeedbackCommand, Response<SkyOnlineRetroFeedbackResponse>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SkyRetroFeedbackCommandHandler> _logger;

        public SkyRetroFeedbackCommandHandler(IMediator mediator, ILogger<SkyRetroFeedbackCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<SkyOnlineRetroFeedbackResponse>> Handle(SkyRetroFeedbackCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var response = await _mediator.Send(new SkyRetroFeedbackRepoCommand(request.FeedbackRequest), cancellationToken);

                if (!response.IsSuccess)
                {
                    _logger.LogInformation($"Claim eligibility failed due to denial code: {response.EligibilityDenialCode} for Unique Identification Number: {request.FeedbackRequest.UniqueIdentificationNumber}");
                    return GetErrorResponse(SkyErrorMapper.RetroError(response.EligibilityDenialCode, null));
                }

                return new Response<SkyOnlineRetroFeedbackResponse>()
                {
                    Status = Status.OK,
                    Value = response,
                    Errors = null
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "SkyRetroFeedback - Exception");
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private static Response<SkyOnlineRetroFeedbackResponse> GetErrorResponse(string errorCode)
        {
            return new Response<SkyOnlineRetroFeedbackResponse>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapTechnicalExceptionToEAPIException(errorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }
    }
}
