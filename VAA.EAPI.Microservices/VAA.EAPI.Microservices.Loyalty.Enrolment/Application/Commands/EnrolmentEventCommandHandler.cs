using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands
{

    public class EnrolmentEventCommandHandler : IRequestHandler<EnrolmentEventRequest, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnrolmentEventCommandHandler> _logger;

        public EnrolmentEventCommandHandler(IMediator mediator, ILogger<EnrolmentEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(EnrolmentEventRequest enrolmentRequest, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.CreateCustomerEventSubject
            };

            try
            {
                var result = ValidateRequest(enrolmentRequest);

                if (result.Value.Status == Status.OK)
                {
                    bool checkEventStored = false;
                    var enrolmentRequestEvent = CreateCustomerEventDataMapper.CustomerDataMapper(enrolmentRequest.Id, enrolmentRequest.Type, enrolmentRequest.Data);
                    try
                    {
                        trackAnalyticsEventCommand.Track("EnrolmentId", enrolmentRequest.Id);
                        trackAnalyticsEventCommand.Track("EnrolmentType", enrolmentRequest.Type);
                        trackAnalyticsEventCommand.Track("EnrolmentSubject", enrolmentRequest.Subject);

                        await _mediator.Send(new EventRequestInsertCommand { Id = enrolmentRequestEvent.Id, Type = enrolmentRequestEvent.Type, Data = enrolmentRequestEvent.Data, Subject = enrolmentRequestEvent.Subject });
                        checkEventStored = true;

                        await _mediator.Publish(enrolmentRequestEvent, cancellationToken);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"An unexpected error has occured in Enrolment Command Handler : {ex.Message}");
                        if (checkEventStored)
                        {
                            _logger.LogWarning($"Rollback event from Event store due to Error");
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = enrolmentRequestEvent.Id, Type = enrolmentRequestEvent.Type, Data = enrolmentRequestEvent.Data, Subject = enrolmentRequestEvent.Subject });
                        }
                        throw;
                    }

                    var enrolmentResponse = EnrolmentResponseMapper.ResponseMapper(enrolmentRequest.Id, Convert.ToString(HttpStatusCode.Accepted));
                    return enrolmentResponse != null ? Result.Success<Response<string>>(enrolmentResponse) : Result.Failure<Response<string>>(Convert.ToString(HttpStatusCode.InternalServerError));
                }
                else
                {
                    return Result.Success(new Response<string>
                    {
                        Errors = result.Value.Errors,
                        Status = Status.BusinessException
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception - {ex}");
                return Result.Failure<Response<string>>((ex.InnerException ?? ex).ToString());
            }
            finally
            {
                _mediator.Send(trackAnalyticsEventCommand, cancellationToken);
            }
        }

        private Result<Response<EnrolmentResponse>> ValidateRequest(EnrolmentEventRequest enrolmentRequest)
        {
            var response = new Response<EnrolmentResponse>()
            {
                Status = Status.OK,
                Value = null,
                Errors = null
            };

            if (string.IsNullOrEmpty(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel))
            {
                response = new Response<EnrolmentResponse>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidChannelError), Guid.NewGuid().ToString())
                };
            }
            else if (enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel == ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                var errors = VredValidations.Validate(enrolmentRequest);
                if (errors.Count > 0)
                {
                    response = new Response<EnrolmentResponse>()
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = errors
                    };
                }
            }
            else if (!new EnrolmentCommandValidator().Validate(enrolmentRequest.Data).IsValid)
            {
                response = new Response<EnrolmentResponse>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = Helper.ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString())
                };
            }

            return response;
        }
    }
}
