using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Application;

public sealed class SubmitRefundCommandHandler : ISubmitRefundCommandHandler
{
    private readonly ID365RefundGateway _d365RefundGateway;
    private readonly IRefundNotificationPublisher _refundNotificationPublisher;
    private readonly IRefundStatusCommandHandler _refundStatusCommandHandler;

    public SubmitRefundCommandHandler(ID365RefundGateway d365RefundGateway, IRefundNotificationPublisher refundNotificationPublisher, IRefundStatusCommandHandler refundStatusCommandHandler)
    {
        _d365RefundGateway = d365RefundGateway ?? throw new ArgumentNullException(nameof(d365RefundGateway));
        _refundNotificationPublisher = refundNotificationPublisher ?? throw new ArgumentNullException(nameof(refundNotificationPublisher));
        _refundStatusCommandHandler = refundStatusCommandHandler ?? throw new ArgumentNullException(nameof(refundStatusCommandHandler));
    }

    public async Task<RefundSubmissionResponse> HandleAsync(RefundRequestForm request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Recheck to ensure that none of the entries have a claim number assigned before submission. This is to prevent any potential issues with concurrent submissions or changes to the refund form after initial validation.
        var entryCheck = await _refundStatusCommandHandler.HandleAsync(request, cancellationToken);
        if (entryCheck.Entries.Any(x => !x.ClaimNumber.IsNullOrEmpty()))
        {
            throw new InvalidOperationException("You cannot submit a refund for entries that already have a claim number.");
        }

        // Submit refund to D365
        var referenceNumber = await _d365RefundGateway.SubmitRefundAsync(request, cancellationToken);

        // Publish notification for submitted refund
        if (!referenceNumber.IsNullOrEmpty())
        {

            await _refundNotificationPublisher.HandleAsync(request, referenceNumber, cancellationToken);


            // TODO: Find all the supporting documents and upload them to D365





            return new RefundSubmissionResponse { ClaimNumber = referenceNumber };
        }

        throw new InvalidOperationException("Failed to submit refund.");
    }
}
