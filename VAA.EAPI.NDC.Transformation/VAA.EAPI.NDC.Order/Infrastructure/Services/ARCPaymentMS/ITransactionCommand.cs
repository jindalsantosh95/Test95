using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.ARCPaymentMS
{
    public interface ITransactionCommand
    {
        Task<TransactionServiceResponse> Handle(TransactionServiceRequest request);
    }
}
