using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class VaEnrolmentResponseCommandHandler : IRequestHandler<VaEnrolmentResponseCommand, Result<Response<VaEnrolmentResponseResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VaEnrolmentResponseCommandHandler> _logger;

        public VaEnrolmentResponseCommandHandler(IMediator mediator, ILogger<VaEnrolmentResponseCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<VaEnrolmentResponseResponse>>> Handle(VaEnrolmentResponseCommand request, CancellationToken cancellationToken)
        {

            Response<VaEnrolmentResponseResponse> deDupResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = Constants.Application.DeDupValidateEventType });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            deDupResponse = VaEnrolmentResponseMapper(result);
                            responseProcessed = true;
                            break;

                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            deDupResponse = VaEnrolmentErrorResponseMapper(result);
                            break;
                    }
                    retryCounter++;
                }

                return deDupResponse != null ? Result.Success(deDupResponse) : Result.Failure<Response<VaEnrolmentResponseResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured while updating dedup response for CorrelationId:{request.CorrelationId}");

                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private static Response<VaEnrolmentResponseResponse> GetErrorResponse(string errorCode)
        {
            return new Response<VaEnrolmentResponseResponse>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }

        private static Response<VaEnrolmentResponseResponse> VaEnrolmentErrorResponseMapper (ServiceResponse serviceResponse)
        {
            return new Response<VaEnrolmentResponseResponse>
            {
                Status = Status.BusinessException,
                Errors = Json.Deserialize<VaEnrolmentResponseResponse>(serviceResponse.JsonResponseData).Errors.ToList(),
                Value = null
            };
        }

        private static Response<VaEnrolmentResponseResponse> VaEnrolmentResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<VaEnrolmentResponseResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<VaEnrolmentResponseResponse>(serviceResponse.JsonResponseData)
            };
        }

    }
}
