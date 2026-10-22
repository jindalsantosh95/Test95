using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.ARCPaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PaymentMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.ARCPaymentMS
{
    public class TransactionRequestMapper
    {
        public static TransactionServiceRequest MapToTransactionRequest(OrderDocument orderDocument, PaymentRS paymentRS, IEnumerable<EAPI.Domain.Virgin.Models.Ticket.AccountableDocumentRef> accountableDocumentRefs, Constants.ProductType productType, [Optional] bool isCashPayment, [Optional] bool isEvenExchange, [Optional] bool isRfc, [Optional] SeatFulfilmentResponse seatPaymentDetails, [Optional] bool isNonRfc, [Optional] EAPI.Domain.Virgin.Models.Payment.PaymentDetails paymentDetails, [Optional] bool isNameCorrection, [Optional] ARCDocument aRCDocuments, [Optional] bool isAddcollect)
        {
            var segmentIds = accountableDocumentRefs?.SelectMany(a => a.Coupons.Select(c => c.SegmentRefId));
            
            var transactionRequest = new TransactionServiceRequest()
            {
                Pos = orderDocument.PointOfSale,
                Parties = orderDocument.Parties,
                ProductType = productType,
                BookingReferences = new()
                {
                    RLOC = orderDocument.BookingReference.RLOC
                },
                OrderItems = orderDocument.OrderItems,
                PaymentDetails = [new()
                {
                   ApprovalCode = paymentRS?.PaymentDocument?.ApprovalCode,
                   FormOfPayments =  isCashPayment ? EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum :EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CardEnum,
                   Amount = new()
                   {
                        MonetaryAmount = paymentRS?.PaymentDocument?.Amount != null ? ((int)(paymentRS.PaymentDocument.Amount * 100)).ToString(): null,
                        BaseCurrency = orderDocument.OrderItems.First().PriceDetails.First().Amount.BaseCurrency
                   },
                   CardMethod = new()
                   {
                       CardNumber = paymentRS ?.PaymentDocument?.MaskedCardNumber,
                       ExpiryDate = paymentRS ?.PaymentDocument?.ExpiryDate,
                       CardType = (!isCashPayment && paymentRS != null) ? Enum.TryParse<CardDetails.CardTypes>(paymentRS ?.PaymentDocument?.CardType, true, out CardDetails.CardTypes result) ? result : CardDetails.CardTypes.VI : null,
                   }
                }],
                TicketStockNumber = ConfigReader.Get(Constants.Environment.TicketStockNumber),
                AccountableDocumentRef = accountableDocumentRefs,
                DataList = new()
                {
                    CustomerDetails = orderDocument.DataList.CustomerDetails,
                    SegmentDetails = productType == Constants.ProductType.Seat ? orderDocument.DataList.SegmentDetails.Where(a=> segmentIds.Contains(a.SegmentId)) : orderDocument.DataList.SegmentDetails,
                    OriginDestinationAssociations = orderDocument.DataList.OriginDestinationAssociations,
                    Penalty = orderDocument?.DataList?.Penalty
                },
                ARCRequestType = isEvenExchange ? Constants.ARCRequestType.EvenExchange : isRfc ? Constants.ARCRequestType.Rfc : isNonRfc ? Constants.ARCRequestType.NonRfc : isNameCorrection ? Constants.ARCRequestType.NameCorrection :  isAddcollect ? Constants.ARCRequestType.AddCollect :null,
                SeatDetails = seatPaymentDetails?.OrderItems,
                ARCDocument = aRCDocuments
            };

            return transactionRequest;
        }
    }
}
