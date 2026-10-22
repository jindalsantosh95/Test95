using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.PaymentMS
{
    public interface ISplitPnrPaymentCommand
    {
        Task<SplitPaymentRS> Handle(SplitPnrPaymentRequest request);
    }
}
