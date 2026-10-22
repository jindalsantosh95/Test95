using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Application
{
    public interface IOrderChangeNotifCommand
    {
        Task<Result<Response<OrderChangeNotifResponse>>> Handle(OrderChangeNotifRequest request);
    }
}
