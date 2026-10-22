using System.Collections;
using System.Runtime.InteropServices;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public interface IOrderResponseMapper
    {
        public IATA_OrderViewRS MapOrderResponse(OrderDocument orderDocument, [Optional] System.Collections.Generic.List<EAPI.Domain.Entities.Common.NDCWarning> nDCWarnings, [Optional] bool isHoldBooking, [Optional] bool isOrderChange, [Optional] bool isOrderCancel, [Optional] bool isNameCorrection);
    }
}
