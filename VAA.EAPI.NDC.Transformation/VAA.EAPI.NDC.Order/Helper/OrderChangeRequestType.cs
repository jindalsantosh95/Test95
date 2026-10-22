using System.Linq;
using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;

namespace VAA.EAPI.NDC.Order.Helper
{
    public static class OrderChangeRequestType
    {
        public static OrderChangeType IdentifyRequestType(IATA_OrderChangeRQ orderChangeReq, AugmentationPoint augmentationPoint)
        {
            var response =  new OrderChangeType();

            if (augmentationPoint?.PaxRefID != null)
            {
                response = OrderChangeType.SplitPnr;
            }
            else
            {
                if ((orderChangeReq.Request?.ChangeOrderChoice?.Item) as SelectedOrderItemType != null)
                {
                    response = OrderChangeType.Disruption;
                }
                else if ((orderChangeReq.Request?.ChangeOrderChoice?.Item) as CancelUnpaidOrderType != null)
                {
                    response = OrderChangeType.CancelHoldBooking;
                }
                else if (orderChangeReq.Request?.ChangeOrderChoice == null)
                {
                    response = OrderChangeType.HoldBooking;
                }
                else if ((orderChangeReq.Request?.ChangeOrderChoice != null) && ((orderChangeReq.Request?.ChangeOrderChoice?.Item) as OrderChangeUpdatePaxType) != null)
                {
                    response = OrderChangeType.AmendApis;
                }
                else if ((orderChangeReq.Request?.ChangeOrderChoice != null) && ((orderChangeReq.Request?.ChangeOrderChoice?.Item) as AcceptCancelOfferType) != null)
                {
                    response = OrderChangeType.Cancel;
                }
                else if (((orderChangeReq.Request.ChangeOrderChoice?.Item) as AcceptSelectedQuotedOfferListType)?.SelectedPricedOffer?.Any(po => po.SelectedOfferItem?.Any(soi => soi.SelectedSeat != null) == true) == true)
                {
                    response = OrderChangeType.PostSaleSeat;
                }
                else if (!(bool)(((orderChangeReq.Request.ChangeOrderChoice?.Item) as AcceptSelectedQuotedOfferListType)?.SelectedPricedOffer?.Any(off => off?.SelectedOfferItem?.FirstOrDefault()?.SelectedALaCarteOfferItem == null)))
                {
                    response = OrderChangeType.HoldBooking;
                }
                else if (orderChangeReq.Request.PaymentFunctions != null && orderChangeReq.Request.PaymentFunctions.Any(x => x.PaymentProcessingDetails.Amount.Value > 0))
                {
                    response = OrderChangeType.AddCollect;
                }
                else if (orderChangeReq.Request.PaymentFunctions != null && orderChangeReq.Request.PaymentFunctions.Any(x => x.PaymentProcessingDetails.Amount.Value == 0))
                {
                    response = OrderChangeType.EvenExchange;
                }
                else
                {
                    response = OrderChangeType.Refund;
                }
            }

            return response;
        }

    }
}
