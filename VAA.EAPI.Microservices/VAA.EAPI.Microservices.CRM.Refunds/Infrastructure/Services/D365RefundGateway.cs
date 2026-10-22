using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Services;

public sealed class D365RefundGateway : ID365RefundGateway
{
    private readonly IHttpConnector _httpConnector;

    public D365RefundGateway(IHttpConnector httpConnector)
    {
        _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
    }

    public async Task<string> GetRefundStatusAsync(RefundRequestItem request, CancellationToken cancellationToken)
    {
        // TODO: Connect to Dynamics 365 and query to see if this ticket / coupon as an existing claim (no matter what state it is in)
        System.Diagnostics.Debug.WriteLine($"Checking refund status for flight number: {request.FlightNumber}, ticket number: {request.TicketNumber}, coupon number: {request.CouponNumber}");

        // TODO: Extract any claim reference from the response


        // This is mocked response
        var claimNumber = request.FlightNumber == 1 ? string.Empty : "some-claim-number";

        return claimNumber;
    }

    public async Task<string> SubmitRefundAsync(RefundRequestForm request, CancellationToken cancellationToken)
    {
        // TODO: Connect to Dynamics 365 and upload the details of the claim
        System.Diagnostics.Debug.WriteLine($"Submitting refund for flight number");

        // TODO: Extract any claim reference from the response

        // This is a mocked response
        var claimNumber = Guid.NewGuid().ToString();


        return claimNumber;
    }

    public Task UploadDocument(string claimRef, byte[] document, CancellationToken cancellationToken)
    {
        // TODO: Connect to Dynamics 365 and upload the document against the claim reference
        System.Diagnostics.Debug.WriteLine($"Uploading document for claim reference: {claimRef}");

        return Task.CompletedTask;
    }

}
