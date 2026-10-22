using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.Air4OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Mapper.Order
{
    public interface IGdsOrderResponseMapper
    {
        public IATA_OrderViewRS MapOrderResponseForGDS(OrderSearchResponse orderSearchResponse, IEnumerable<NDCWarning> warnings = null);
        public OrderSearchResponse AddSeatOrderItems(OrderChangeResponse orderChangeResponse, OrderSearchResponse orderSearchResponse, IEnumerable<SeatDetails> seatDetailsForGds, IEnumerable<SeatTransactionId> transactionIds, IEnumerable<TransactionIdStatus> transactionStatus, PaymentRS paymentRS);
    }
}
