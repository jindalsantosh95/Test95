using System.Threading.Tasks;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS
{
    public interface IPaymentMSCaptureCommand
    {
        Task<PaymentMSResponse> Handle(CapturePaymentRequest request);
    }
}
