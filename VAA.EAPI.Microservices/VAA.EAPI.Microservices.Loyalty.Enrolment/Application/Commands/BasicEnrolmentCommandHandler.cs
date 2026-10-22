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
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands
{
    public class BasicEnrolmentCommandHandler : IRequestHandler<BasicEnrolmentCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BasicEnrolmentCommandHandler> _logger;

        public BasicEnrolmentCommandHandler(IMediator mediator, ILogger<BasicEnrolmentCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<string>>> Handle(BasicEnrolmentCommand request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = "Basic" + Constants.Application.CreateCustomerEventSubject
            };

            try
            {
                trackAnalyticsEventCommand.Track("ChanelId", request.ChannelId);

                if (request.EnrolmentRequest.CustomerDetails == null)
                    request.EnrolmentRequest.CustomerDetails = new Domain.CustomerDetails.Customer();

                if (request.EnrolmentRequest.CustomerDetails.LoyaltyMember != null)
                    request.EnrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel = request.ChannelId;
                else
                    request.EnrolmentRequest.CustomerDetails.LoyaltyMember = new() { EnrollmentChannel = request.ChannelId };

                var result = ValidateRequest(request.EnrolmentRequest);

                if (result.Value.Status == Status.OK)
                {
                    var enrolmentRequestEvent = CreateCustomerEventDataMapper.CustomerDataMapper(Convert.ToString(Guid.NewGuid()), ConfigReader.Get(Constants.Environment.EnrolmentEventType), request.EnrolmentRequest);

                    trackAnalyticsEventCommand.Track("EnrolmentId", enrolmentRequestEvent.Id);
                    trackAnalyticsEventCommand.Track("EnrolmentType", enrolmentRequestEvent.Type);
                    trackAnalyticsEventCommand.Track("EnrolmentSubject", enrolmentRequestEvent.Subject);

                    try
                    {
                        await _mediator.Send(new EventRequestInsertCommand { Id = enrolmentRequestEvent.Id, Type = enrolmentRequestEvent.Type, Data = enrolmentRequestEvent.Data, Subject = enrolmentRequestEvent.Subject });
                        try
                        {
                            await _mediator.Publish(enrolmentRequestEvent, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = enrolmentRequestEvent.Id, Type = enrolmentRequestEvent.Type, Data = enrolmentRequestEvent.Data, Subject = enrolmentRequestEvent.Subject });
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"An unexpected error has occured in Basic Enrolment Command Handler : {ex.Message}");
                        throw;
                    }

                    return new Response<string>
                    {
                        Status = Status.OK,
                        Value = string.Empty
                    };
                }
                else
                {
                    _logger.LogInformation(Json.Serialize(result.Value.Errors));

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
                return Result.Failure<Response<string>>(ex.InnerException.ToString());
            }
            finally
            {
                _mediator.Send(trackAnalyticsEventCommand, cancellationToken);
            }
        }

        private Result<Response<EnrolmentResponse>> ValidateRequest(EnrolmentRequest enrolmentRequest)
        {
            var response = new Response<EnrolmentResponse>()
            {
                Status = Status.OK,
                Value = null,
                Errors = null
            };

            if (string.IsNullOrEmpty(enrolmentRequest?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel) || string.IsNullOrEmpty(Convert.ToString(enrolmentRequest?.CustomerDetails?.CustomerType)))
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
