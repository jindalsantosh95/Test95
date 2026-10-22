using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Application.OrderCreate
{
    public interface IOrderCreateEasyPayCommand
    {
        Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderCreateRQ request, bool isEasyPay, string subscriptionKey, CancellationToken cancellationToken);
    }
}
