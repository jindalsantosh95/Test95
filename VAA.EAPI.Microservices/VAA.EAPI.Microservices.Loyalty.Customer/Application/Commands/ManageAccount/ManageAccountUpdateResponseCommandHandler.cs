using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class ManageAccountUpdateResponseCommandHandler : IRequestHandler<ManageAccountUpdateResponseCommand, Result<Response<ManageAccountUpdateResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ManageAccountUpdateResponseCommandHandler> _logger;

        public ManageAccountUpdateResponseCommandHandler(IMediator mediator, ILogger<ManageAccountUpdateResponseCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<ManageAccountUpdateResponse>>> Handle(ManageAccountUpdateResponseCommand request, CancellationToken cancellationToken)
        {

            Response<ManageAccountUpdateResponse> updateResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.UpdateType });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            updateResponse = ManageAccountUpdateResponseMapper(result);
                            responseProcessed = true;
                            break;

                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
                    }
                    retryCounter++;
                }

                return updateResponse != null ? Result.Success(updateResponse) : Result.Failure<Response<ManageAccountUpdateResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured while updating Address for CorrelationId:{request.CorrelationId}");

                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private static Response<ManageAccountUpdateResponse> GetErrorResponse(string errorCode)
        {
            return new Response<ManageAccountUpdateResponse>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }

        public static Response<ManageAccountUpdateResponse> ManageAccountUpdateResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<ManageAccountUpdateResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<ManageAccountUpdateResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}
