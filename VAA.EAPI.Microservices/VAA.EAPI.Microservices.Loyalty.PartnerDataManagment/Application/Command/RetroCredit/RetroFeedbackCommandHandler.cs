using MediatR;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Validations;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit
{
    public class RetroFeedbackCommandHandler : IRequestHandler<RetroFeedbackCommand, Response<RetroFeedbackResponse>>
    {

        private readonly IFlownDbRepository _flownDBRepository;

        private readonly ILogger<RetroFeedbackCommandHandler> _logger;

        public RetroFeedbackCommandHandler(IFlownDbRepository flownDBRepository, ILogger<RetroFeedbackCommandHandler> logger)
        {
            _flownDBRepository = flownDBRepository ?? throw new ArgumentNullException(nameof(flownDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response<RetroFeedbackResponse>> Handle(RetroFeedbackCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var feedbackRequestValidation = new FeedbackRequestValidator().Validate(request.RetroFeedbackRequest);

                if (feedbackRequestValidation.IsValid)
                {

                    bool IsUpdated = await _flownDBRepository.UpdateFlownDetailsWithRetroFeedbackAsync(request.RetroFeedbackRequest);

                    if (IsUpdated)
                    {
                        return new Response<RetroFeedbackResponse>
                        {
                            Status = Status.OK,
                            Value = new RetroFeedbackResponse
                            {
                                EligibilityDenialCode = Constants.ErrorCodes.EDC999,
                                TrackID = request.RetroFeedbackRequest.TrackID,
                                UniqueIdentificationNumber = request.RetroFeedbackRequest.UniqueIdentificationNumber
                            }

                        };
                    }
                    else
                    {
                        return new Response<RetroFeedbackResponse>
                        {
                            Status = Status.BusinessException,
                            Value = new RetroFeedbackResponse
                            {
                                EligibilityDenialCode = Constants.ErrorCodes.EDC107,
                                TrackID = request.RetroFeedbackRequest.TrackID,
                                UniqueIdentificationNumber = request.RetroFeedbackRequest.UniqueIdentificationNumber

                            }
                        };
                    }

                }
                else
                {
                    return new Response<RetroFeedbackResponse>
                    {
                        Status = Status.BusinessException,
                        Value = new RetroFeedbackResponse
                        {
                            EligibilityDenialCode = Constants.ErrorCodes.EDC997,
                            TrackID = request.RetroFeedbackRequest.TrackID,
                            UniqueIdentificationNumber = request.RetroFeedbackRequest.UniqueIdentificationNumber
                        }
                    };
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "RetroFeedbackCommandHandler:SqlException");

                return new Response<RetroFeedbackResponse>
                {
                    Status = Status.Error,
                    Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.Tech_Error,
                             ErrorSource = ex.Message,
                             Code = ex.Number.ToString()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RetroFeedbackCommandHandler:Exception");

                return new Response<RetroFeedbackResponse>
                {
                    Status = Status.Error,
                    Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                             Message = Constants.ErrorCodes.Tech_Error,
                             ErrorSource = ex.Message,
                        }
                    }
                };
            }
        }
    }

}



