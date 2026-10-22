using VAA.EAPI.Microservices.CRM.Refunds.Application.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

namespace VAA.EAPI.Microservices.CRM.Refunds.Application;

public sealed class RefundStatusCommandHandler : IRefundStatusCommandHandler
{
    private readonly ID365RefundGateway _d365RefundGateway;

    public RefundStatusCommandHandler(ID365RefundGateway d365RefundGateway)
    {
        _d365RefundGateway = d365RefundGateway ?? throw new ArgumentNullException(nameof(d365RefundGateway));
    }

    public async Task<RefundRequestForm> HandleAsync(RefundRequestForm request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tasks = request.Entries
            .Select(async entry =>
            {
                entry.ClaimNumber = await _d365RefundGateway.GetRefundStatusAsync(entry, cancellationToken);
            })
            .ToArray();

        await Task.WhenAll(tasks);

        return request;
    }
}
