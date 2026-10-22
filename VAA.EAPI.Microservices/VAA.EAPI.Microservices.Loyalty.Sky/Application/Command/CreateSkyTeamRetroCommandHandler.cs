using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Sky.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Application.Command
{
    public class CreateSkyTeamRetroCommandHandler : IRequestHandler<CreateSkyTeamRetroCommand, Response<CheckEligibilityResponse>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateSkyTeamRetroCommandHandler> _logger;

        public CreateSkyTeamRetroCommandHandler(IMediator mediator, ILogger<CreateSkyTeamRetroCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<Response<CheckEligibilityResponse>> Handle(CreateSkyTeamRetroCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var response = await _mediator.Send(new SkyTeamEligibiltyRepoCommand(request.TicketInfo), cancellationToken);

                if ((response.EligibilityDenialCode == "000" || response.EligibilityDenialCode == "001") && response.SkyClaimResponse == null)
                {
                    _logger.LogInformation($"Claim eligibility failed due to denial code: 301 for Ticket Number : {request.TicketInfo.EticketNumber}");
                    return GetErrorResponse(SkyErrorMapper.RetroError("301", response.SkyClaimResponse?.PostingNonEligibilityIndicator), response);
                }

                if (!response.IsEligible)
                {
                    _logger.LogInformation($"Claim eligibility failed due to denial code: {response.EligibilityDenialCode}{response.SkyClaimResponse?.PostingNonEligibilityIndicator} for Ticket Number : {request.TicketInfo.EticketNumber}");
                    return GetErrorResponse(SkyErrorMapper.RetroError(response.EligibilityDenialCode, response.SkyClaimResponse?.PostingNonEligibilityIndicator), response);
                }

                return new Response<CheckEligibilityResponse>()
                {
                    Status = Status.OK,
                    Value = response,
                    Errors = null
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "SkyTeamEligibiltyCheck - Exception");
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError, null);
            }
        }

        private static Response<CheckEligibilityResponse> GetErrorResponse(string errorCode, CheckEligibilityResponse response)
        {

            if (errorCode == "S201R" || errorCode == "S201Y")
            {
                var errormsg = ErrorMapper.MapTechnicalExceptionToEAPIException(errorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault();
                ErrorDetails errorDetails = new ErrorDetails();
                errorDetails.Code = errormsg?.Code;
                errorDetails.DeveloperMessage = errormsg?.DeveloperMessage;
                errorDetails.ErrorSource = errormsg?.ErrorSource;
                errorDetails.Message = $"{errormsg?.Message} {response?.SkyClaimResponse?.FFPCarrierCode} {response?.SkyClaimResponse?.FFPAccountNumber}";

                return new Response<CheckEligibilityResponse>()
                {
                    Status = Status.BusinessException,
                    Errors = new()
                {
                    errorDetails
                }

                };
            }
            else
            {
                return new Response<CheckEligibilityResponse>()
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
}
