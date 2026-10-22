using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BirthdayEmail;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class BirthdayEmailCommand : IRequest<string>
    {
        public Guid Id { get; set; }
    }

    public class BirthdayEmailCommandHandler : IRequestHandler<BirthdayEmailCommand, string>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BirthdayEmailCommandHandler> _logger;

        public BirthdayEmailCommandHandler(IMediator mediator, ILogger<BirthdayEmailCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<string> Handle(BirthdayEmailCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Birthday Email Command Handler with Id {request.Id}- Start");
            try
            {
                var memberList = await _mediator.Send(new GetMembersForBirthdayEmailQuery());
                _logger.LogInformation($"For Birthday Email Command - Total {memberList?.Count()} Records Found");
                foreach (var member in memberList)
                {
                    var eventData = BirthdayEmailTriggeringEventMapper.CreateEventGridSchemaData(member);
                    var cloudEvent = BirthdayEmailTriggeringEventGridMapper.EventDataMapper(eventData);
                    _logger.LogDebug($"Store event in event store - Start");
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent, Subject = cloudEvent.Subject });
                    _logger.LogDebug($"Store event in event store - End");
                    try
                    {
                        _logger.LogInformation("Send Email Triggering for Birthday Event-Start");
                        await _mediator.Publish(cloudEvent, CancellationToken.None);
                        _logger.LogInformation("Send Email Triggering for Birthday Event-End");
                    }
                    catch
                    {
                        _logger.LogDebug($"Deleting the event from event store");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                        throw;
                    }
                }

                _logger.LogInformation($"Birthday Email Command Handler {request.Id} - Completed");
                return Constants.Application.BirthdayEmailProcessSuccessMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BirthdayEmailCommandHandler {request.Id} failed due to Exception");
                return Constants.Application.Faliure;
            }
            finally
            {
                _logger.LogInformation($"Birthday Email Command Handler {request.Id} - End");
            }
        }
    }
}
