using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Query
{
    public class EnrolmentResponseQuery : IRequest<Result<Response<EnrolmentResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
    public class EnrolmentResponseQueryHandler : IRequestHandler<EnrolmentResponseQuery, Result<Response<EnrolmentResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnrolmentResponseQueryHandler> _logger;

        public EnrolmentResponseQueryHandler(IMediator mediator, ILogger<EnrolmentResponseQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<Result<Response<EnrolmentResponse>>> Handle(EnrolmentResponseQuery request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.CreateCustomerEventSubject + "ResponseQuery" 
            };

            trackAnalyticsEventCommand.Track("CorrelationId", request.CorrelationId);

            Response<EnrolmentResponse> enrolmentResponse = new Response<EnrolmentResponse>();
            try
            {

                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = Convert.ToInt32(Environment.GetEnvironmentVariable(Constants.Environment.ResponseDelayTimeInMS));
                while (!responseProcessed && retryCounter <= Convert.ToInt32(Environment.GetEnvironmentVariable(Constants.Environment.ResponsePollingCount)))

                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            enrolmentResponse = EnrolmentQueryResponseMapping.EnrolmentQueryResponseMapper(result);
                            responseProcessed = true;
                            break;
                        case ServiceResponseProcessStatus.Error:
                            responseProcessed = true;
                            break;
                    }

                    retryCounter++;
                }

                trackAnalyticsEventCommand.Track("RetryCounter", retryCounter);

                var errorResponse = EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);

                trackAnalyticsEventCommand.Track("Response", enrolmentResponse.Status);
                trackAnalyticsEventCommand.Track("ErrorResponse", errorResponse);

                return enrolmentResponse.Value != null || enrolmentResponse.Errors != null ? Result.Success(enrolmentResponse) : Result.Failure<Response<EnrolmentResponse>>(Json.Serialize(errorResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Result.Failure<Response<EnrolmentResponse>>(ex.Message);
            }
            finally
            {
                _mediator.Send(trackAnalyticsEventCommand, cancellationToken);
            }
        }
    }
}
