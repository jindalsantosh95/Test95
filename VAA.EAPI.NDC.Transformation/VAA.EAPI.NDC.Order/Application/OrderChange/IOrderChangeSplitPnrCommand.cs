using VAA.EAPI.Domain.Entities.Common;
using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Application.OrderChange
{
    public interface IOrderChangeSplitPnrCommand
    {
        Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderChangeRQ request, AugmentationPoint augmentationPoint, string subscriptionKey, CancellationToken cancellationToken);

    }
}
