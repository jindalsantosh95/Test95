using VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Application;

public sealed class RefundNotificationPublisher : IRefundNotificationPublisher
{
    //private readonly IMessageBus _messageBus;

    public RefundNotificationPublisher(/*IMessageBus messageBus*/)
    {
        //_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
    }

    public async Task HandleAsync(RefundRequestForm request, string claimNumber, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // TODO: Publish refund submission message to Service Bus topic for asynchronous processing by other components (e.g. CRM integration, email notification, etc.)
        // TODO: Unit Tests have also been commented out pending full implementation of message bus.

        /*   EXAMPLE */
        /*
        var responseEvent = new CloudEvent()
        {
            Id = Guid.NewGuid().ToString(),
            Subject = Constants.Application.RefundRequestEmailNotificationSubject,
            Type = Constants.Application.RefundRequestEmailNotificationSubject,
            Source = Constants.Application.ServiceBusSource,
            Time = DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt"),
            Data = Json.Serialize(new
            {
                ClaimNumber = claimNumber,
                Request = request
            })
        };

        AsyncContext.Current.MapToCloudEvent(responseEvent);
        await _messageBus.PublishMessageAsync(responseEvent);
        */
    }
}
