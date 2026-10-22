using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Services.VoucherMonitoring
{
    public interface IVoucherMonitoringService
    {
        Task<VoucherMonitoringResponse> GetVoucherTransactionMonitoringStatus(VoucherMonitoringRequest request, CancellationToken cancellationToken);
    }
}

