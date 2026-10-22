using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.CRM.Domain;
using VAA.EAPI.Microservices.CRM.Mapper;

namespace VAA.EAPI.Microservices.CRM.Application.Query
{
    public class PaymentUpdateStatusResponseQueryHandler : IRequestHandler<PaymentUpdateStatusResponseQuery, Result<Response<PaymentResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentUpdateStatusResponseQueryHandler> _logger;
        public PaymentUpdateStatusResponseQueryHandler(IMediator mediator, ILogger<PaymentUpdateStatusResponseQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<Result<Response<PaymentResponse>>> Handle(PaymentUpdateStatusResponseQuery request, CancellationToken cancellationToken)
        {
            Response<PaymentResponse> orderChangeResponse = new();

            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))

                {
                    _logger.LogInformation($"Retry Count:- {retryCounter}");

                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {
                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            orderChangeResponse = PaymentUpdateQueryResponseMapper.MapToPaymentUpdateResponse(result);
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            return Result.Failure<Response<PaymentResponse>>(result.JsonResponseData);
                    }
                    retryCounter++;
                }

                return orderChangeResponse.Value != null ? Result.Success(orderChangeResponse) : Result.Failure<Response<PaymentResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error has occured in CRM Payment Update Response Status Query Handler with Co-relation id:{request.CorrelationId}");
                return Result.Failure<Response<PaymentResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
