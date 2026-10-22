using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.VoucherMonitoring
{
    public interface IVoucherMonitoringHandler
    {
        Task<Result<Response<VoucherMonitoringResponse>>> Handle(VoucherMonitoringEvent monitoringEvent, CancellationToken cancellationToken);
    }
}
