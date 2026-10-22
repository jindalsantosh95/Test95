using System.Threading.Tasks;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS
{
    public interface IPaymentMSAuthorizeCommand
    {
        Task<PaymentMSResponse> Handle(AuthorizePaymentRequest request);
    }
}
