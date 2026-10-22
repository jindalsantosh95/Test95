using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Infrastructure.Model.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.EmailMS
{
    public interface ISendOrderEmailCommand
    {
        Task SendOrderEmail(SendOrderEmailRequest request);
    }
}
