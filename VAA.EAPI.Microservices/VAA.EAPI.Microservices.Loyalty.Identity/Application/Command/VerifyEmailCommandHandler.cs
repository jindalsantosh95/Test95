using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Response<bool>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VerifyEmailCommandHandler> _logger;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public VerifyEmailCommandHandler(
            IMediator mediator,
            ILogger<VerifyEmailCommandHandler> logger,
            ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }

        public async Task<Response<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = "VerifyEmail"
            };

            try
            {
                int retryCounter = ConfigReader.Get<int>(Constants.Environment.RetryCounter);

                await _mediator.Send(new UpdateUserRepoCommand(request.ObjectId), cancellationToken);

                //Verify the activation link               
                var eventData = EmailTriggeringEventRequestMapper.CreateEventGridSchemaDatawithWelcomeEmail(request.CustomerDetails);
                var cloudEvent = EmailTriggeringCloudEventMapper.EventDataMapper(eventData);

                bool isEmailPublished = false;
                for (int i = 0; i < retryCounter; i++)
                {
                    try
                    {
                        await PublishToEmailEvent(cloudEvent, cloudEvent.Id, cancellationToken);
                        isEmailPublished = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Email event publishing failed.");
                    }
                }

                trackAnalyticsEventCommand.Track("IsEmailPublished", isEmailPublished);
                trackAnalyticsEventCommand.Track("RetryCounter", retryCounter);

                return new Response<bool>()
                {
                    Status = Status.OK,
                    Value = isEmailPublished,
                    Errors = null
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email verification failed.");
                return new Response<bool>()
                {
                    Status = Status.BusinessException,
                    Value = false,
                    Errors = null
                }; ;
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(trackAnalyticsEventCommand, cancellationToken);
            }
        }

        private async Task PublishToEmailEvent(CloudEvent emailEvent, string correlationId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = correlationId, Type = Constants.Application.WelcomeEmailEvent, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(emailEvent, cancellationToken);
            }
            catch (Exception)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationId, Type = Constants.Application.WelcomeEmailEvent, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);
                throw;
            }
        }
    }
}
