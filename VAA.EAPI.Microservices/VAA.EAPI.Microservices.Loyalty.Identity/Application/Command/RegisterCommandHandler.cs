using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Response<B2CUserResponse>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public RegisterCommandHandler(
            IMediator mediator,
            ILogger<RegisterCommandHandler> logger,
            ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Response<B2CUserResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = "Identity:Register"
            };

            try
            {
                var emailId = request?.CustomerDetails?.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId.ToLower();

                request.CustomerDetails.Gender = NormalizeGender(request?.CustomerDetails?.Gender);

                var phoneDetails = request?.CustomerDetails?.ContactDetails?.PhoneDetails;
                if (phoneDetails != null)
                {
                    foreach (var p in phoneDetails)
                    {
                        if (!string.IsNullOrWhiteSpace(p.CountryCode) && p.CountryCode.Trim().StartsWith("+"))
                            p.CountryCode = p.CountryCode.Trim().TrimStart('+');
                    }
                }

                var addressDetails = request?.CustomerDetails?.AddressDetails;
                if (addressDetails != null)
                {
                    foreach (var a in addressDetails)
                    {
                        if (a.AddressType == null && a.PreferredFlag == "Y")
                            a.AddressType = AccountAddressDetail.AccountAddressTypes.Home;
                    }
                }

                MemberRegisterRequest memberRegisterRequest = new()
                {
                    MemberNumber = request?.CustomerDetails?.FcNumber,
                    EmailAddress = emailId,
                    Password = request?.CustomerDetails?.Password,
                    FirstName = request?.CustomerDetails?.FirstName,
                    LastName = request?.CustomerDetails?.LastName,
                    CustomerId = request?.CustomerDetails?.CustomerId,
                    CountryCode = request?.CustomerDetails.AddressDetails.First().Country,
                    MemberType = request?.CustomerDetails?.LoyaltyMember?.MemberType.ToString()
                };

                trackAnalyticsEventCommand.Track("MemberType", request?.CustomerDetails?.LoyaltyMember?.MemberType);

                B2CUserResponse response = await _mediator.Send(new RegisterRepoCommand(memberRegisterRequest), cancellationToken);

                trackAnalyticsEventCommand.Track("RegisterSuccessful", response.IsB2COperationSuccess);

                if (!response.IsB2COperationSuccess)
                    return new Response<B2CUserResponse>()
                    {
                        Status = Status.OK,
                        Value = response,
                        Errors = null
                    };

                var correlationIdForEvents = Guid.NewGuid().ToString();
                var emailEventData = EmailTriggeringEventRequestMapper.CreateEventGridSchemaDatawithWelcomeEmail(MapCustomerDetailsToCustomerEntities.MaptoCreateCustomerEntities(request?.CustomerDetails));
                var emailCloudEvent = EmailTriggeringCloudEventMapper.EventDataMapper(emailEventData);

                if (request.CustomerDetails.AddressDetails.Any(x => x.PostalCode.Contains('-')))
                {
                    foreach (var item in request.CustomerDetails.AddressDetails)
                    {
                        item.PostalCode = item.PostalCode.Replace("-", "");
                    }
                }
                var customerSyncCloudEvent = CustomerSyncEventMapper.EventDataMapper(correlationIdForEvents, request.CustomerDetails);
                var vredOcdbSyncCloudEvent = VredOcdbSyncEventMapper.EventDataMapper(correlationIdForEvents, request.CustomerDetails);
                var vredMarketingPreferenceCloudEvent = VredMarketingPreferenceEventMapper.EventDataMapper(correlationIdForEvents, request.CustomerDetails);
                var vredPreferenceData = vredMarketingPreferenceCloudEvent?.Data as ConsentReceiptRequest;

                var tasks = new List<Task>
                {
                     PublishToEnrolCustomerEvent(customerSyncCloudEvent, correlationIdForEvents, cancellationToken),
                     PublishToVredOcDbSync(vredOcdbSyncCloudEvent, correlationIdForEvents, cancellationToken),
                     PublishToEmailEvent(emailCloudEvent, correlationIdForEvents, cancellationToken)
                };

                if (!string.IsNullOrEmpty(vredPreferenceData?.MarketingPreference) && ConfigReader.Get(Constants.Application.VredMarketingFeatureFlag) == "Y")
                {
                    tasks.Add(PublishToMarketingPreferenceEvent(vredMarketingPreferenceCloudEvent, correlationIdForEvents, cancellationToken));
                }

                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while publishing cloud events", ex);
                }

                return new Response<B2CUserResponse>()
                {
                    Status = Status.OK,
                    Value = response,
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure B2C registration failed.");

                return new Response<B2CUserResponse>()
                {
                    Status = Status.OK,
                    Value = new B2CUserResponse
                    {
                        IsB2COperationSuccess = false,
                        B2CFaliureMessage = ex?.Message

                    },
                    Errors = null
                };
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(trackAnalyticsEventCommand, cancellationToken);
            }
        }

        private static string? NormalizeGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return null;

            return gender.ToLowerInvariant() switch
            {
                "female" => "F",
                "male" => "M",
                "undisclosed" => "U",
                "unspecified" => "X",
                _ => gender
            };
        }

        private async Task PublishToVredOcDbSync(CloudEvent vredOcdbSyncCloudEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.VredOcdbSycnEvent, Data = vredOcdbSyncCloudEvent.Data, Subject = vredOcdbSyncCloudEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(vredOcdbSyncCloudEvent, cancellationToken);
            }
            catch (Exception)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.VredOcdbSycnEvent, Data = vredOcdbSyncCloudEvent.Data, Subject = vredOcdbSyncCloudEvent.Subject }, cancellationToken);
                throw;
            }
        }
        private async Task PublishToEmailEvent(CloudEvent emailEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            int retryCounter = ConfigReader.Get<int>(Constants.Environment.RetryCounter);

            for (int i = 0; i < retryCounter; i++)
            {

                await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.WelcomeEmailEvent, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);
                try
                {
                    await _mediator.Publish(emailEvent, cancellationToken);
                    break;
                }
                catch (Exception ex)
                {
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.WelcomeEmailEvent, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);
                    _logger.LogError(ex, "Email event publishing failed for {CorrelationId}.", correlationIdForEvents);
                }

            }
        }
        private async Task PublishToEnrolCustomerEvent(CloudEvent enrolCustomerEventData, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.EnrolCustomerEvent, Data = enrolCustomerEventData.Data, Subject = enrolCustomerEventData.Subject }, cancellationToken);
            try
            {
                await _mediator.Publish(enrolCustomerEventData, cancellationToken);
            }
            catch (Exception ex)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.SignUpEmailEvent, Data = enrolCustomerEventData.Data, Subject = enrolCustomerEventData.Subject }, cancellationToken);
                _logger.LogError(ex, "Email event publishing failed ", ex?.Message);
            }
        }

        private async Task PublishToMarketingPreferenceEvent(CloudEvent enrolCustomerEventData, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = enrolCustomerEventData.Type, Data = enrolCustomerEventData.Data, Subject = enrolCustomerEventData.Subject }, cancellationToken);
            try
            {
                await _mediator.Publish(enrolCustomerEventData, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Marketing preference event publishing failed.", ex?.Message);
            }
        }
    }
}

