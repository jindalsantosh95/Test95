using System;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class NtdhMapper
    {
        public static CloudEvent EventMapper(NdcTicketDataHandoff request, [Optional] bool isRefund)
        {
            return new CloudEvent()
            {
                Data = Xml.Serialize(request),
                DataContentType = Constants.Application.xmlContentType,
                Type = isRefund ? Constants.Application.NtdhRefundEventType : Constants.Application.NtdhEventType,
                Source = Constants.Application.NtdhTopicName,
                Subject = Constants.Application.NtdhEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventUtcTimeFormat)
            };
        }

        public static NdcTicketDataHandoff RequestMapper(Domain.CPD.PaymentTransactionDocument paymentDoc)
        {
            return new NdcTicketDataHandoff()
            {
                Rxoi = new RxoiNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxoi,
                    Channel = Constants.Application.NdthChannel,
                    PssPnrLocator = paymentDoc.BookingReferences.ExternalRecordLocator,
                    DocumentOrderReference = paymentDoc.OrderId,
                    PaymentDocumentReferenceIndicator = paymentDoc.Id,
                    PaymentCardMaskedNumber = $"{paymentDoc.PaymentDetails.First().CardMethod.CardType}{paymentDoc.PaymentDetails.First().CardMethod.CardNumber}",
                    PaymentApprovalCode = paymentDoc.PaymentDetails.First().PaymentApprovalCode
                },
                Rxsa = new RxsaNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxsa,
                    PaymentOrchistrationProvider = Constants.Application.paymentOrchestrationProvider,
                    RouteId = paymentDoc.PaymentDetails.First().Acquirer.ToUpper(),
                    PaymentMethod = string.Empty
                },
                Rxpr = new RxprNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxpr,
                    ProviderId = paymentDoc.Id
                }
            };
        }

        public static NdcTicketDataHandoff RefundRequestMapper(PaymentTransactionDocument paymentDoc, string paymentDocId)
        {
            return new NdcTicketDataHandoff()
            {
                Rxoi = new RxoiNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxoi,
                    Channel = Constants.Application.NdthChannel,
                    PssPnrLocator = paymentDoc.BookingReferences.ExternalRecordLocator,
                    DocumentOrderReference = paymentDoc.OrderId,
                    PaymentDocumentReferenceIndicator = paymentDocId,
                    PaymentCardMaskedNumber = $"{paymentDoc.PaymentDetails.First().CardMethod.CardType}{paymentDoc.PaymentDetails.First().CardMethod.CardNumber}",
                    PaymentApprovalCode = paymentDoc.PaymentDetails.First().PaymentApprovalCode
                },
                Rxsa = new RxsaNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxsa,
                    PaymentOrchistrationProvider = Constants.Application.paymentOrchestrationProvider,
                    RouteId = paymentDoc.PaymentDetails.First().Acquirer.ToUpper(),
                    PaymentMethod = string.Empty
                },
                Rxpr = new RxprNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxpr,
                    ProviderId = paymentDocId
                }
            };
        }

        public static NdcTicketDataHandoff CashRequestMapper(CreatePaymentDocumentRequest paymentDoc)
        {
            return new NdcTicketDataHandoff()
            {
                Rxoi = new RxoiNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxoi,
                    Channel = Constants.Application.NdthChannel,
                    PssPnrLocator = paymentDoc.BookingReferences.ExternalRecordLocator,
                    DocumentOrderReference = paymentDoc.OrderId,
                    PaymentDocumentReferenceIndicator = paymentDoc.PaymentDocumentId,
                    PaymentCardMaskedNumber = paymentDoc.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay ? paymentDoc.PaymentDetails.First().CardMethod.CardNumber : string.Empty,
                    PaymentApprovalCode = paymentDoc.PaymentDetails.First().FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay ? paymentDoc.PaymentDetails.First().PaymentApprovalCode : string.Empty
                },
                Rxsa = new RxsaNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxsa,
                    PaymentOrchistrationProvider = Constants.Application.paymentOrchestrationProviderBSP,
                    RouteId = Constants.Application.IFG,
                    PaymentMethod = string.Empty
                },
                Rxpr = new RxprNdcTicketDataHandoff()
                {
                    DataType = Constants.Application.Rxpr,
                    ProviderId = paymentDoc.PaymentDocumentId
                }
            };
        }

    }
}
