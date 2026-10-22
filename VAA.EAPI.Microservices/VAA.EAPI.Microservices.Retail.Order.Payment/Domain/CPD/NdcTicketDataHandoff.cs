using System;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{

    [XmlRoot(ElementName = "NDC_Ticket_Data_Handoff")]
    public class NdcTicketDataHandoff
    {
        [XmlElement(ElementName = "RXOI_NDC_Ticket_Data_Handoff")]
        public RxoiNdcTicketDataHandoff Rxoi { get; set; }

        [XmlElement(ElementName = "RXSA_NDC_Ticket_Data_Handoff")]
        public RxsaNdcTicketDataHandoff Rxsa { get; set; }

        [XmlElement(ElementName = "RXPR_NDC_Ticket_Data_Handoff")]
        public RxprNdcTicketDataHandoff Rxpr { get; set; }
    }

    public class RxoiNdcTicketDataHandoff
    {
        [XmlElement(ElementName = "Data_Type")]
        public string DataType { get; set; }

        [XmlElement(ElementName = "Channel")]
        public string Channel { get; set; }

        [XmlElement(ElementName = "PSS_PNR_Locator")]
        public string PssPnrLocator { get; set; }

        [XmlElement(ElementName = "Document_Order_Reference")]
        public string DocumentOrderReference { get; set; }

        [XmlElement(ElementName = "Payment_Document_Reference_Indicator")]
        public string PaymentDocumentReferenceIndicator { get; set; }

        [XmlElement(ElementName = "Payment_Card_Masked_Number")]
        public string PaymentCardMaskedNumber { get; set; }

        [XmlElement(ElementName = "Payment_Approval_Code")]
        public string PaymentApprovalCode { get; set; }
    }

    public class RxsaNdcTicketDataHandoff
    {
        [XmlElement(ElementName = "Data_Type")]
        public string DataType { get; set; }

        [XmlElement(ElementName = "Payment_Orchistration_Provider")]
        public string PaymentOrchistrationProvider { get; set; }

        [XmlElement(ElementName = "Route_ID")]
        public string RouteId { get; set; }

        [XmlElement(ElementName = "Payment_Method")]
        public string PaymentMethod { get; set; }
    }

    public class RxprNdcTicketDataHandoff
    {
        [XmlElement(ElementName = "Data_Type")]
        public string DataType { get; set; }

        [XmlElement(ElementName = "Provider_ID")]
        public string ProviderId { get; set; }
    }

}
