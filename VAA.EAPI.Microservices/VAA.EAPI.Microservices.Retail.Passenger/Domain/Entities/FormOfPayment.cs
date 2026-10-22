namespace VAA.EAPI.Microservices.Retail.Passenger.Domain.Entities
{
    public class FormOfPayment
    {
        public string AdditionalCollectionFlag { get; set; }
        public string AmericanExpressPaymentFlag { get; set; }
        public string CardHolderNameOrOther { get; set; }
        public string CreditCardApprovalCode { get; set; }
        public string CreditCardExpiration { get; set; }
        public string CreditCardNumber { get; set; }
        public string FirstNameNumber { get; set; }
        [System.Xml.Serialization.XmlAttribute("FormOfPayment")]
        public string FormOfPayment1 { get; set; }
        public string FreeformText { get; set; }
        public string LastNameNumber { get; set; }
        public string NonRefundableFlag { get; set; }
        public string PaymentDueFlag { get; set; }
        public string PrepaidTicketAdviceFlag { get; set; }
        public string RecordName { get; set; }
        public string RefundFlag { get; set; }
        public string RemarkNumber { get; set; }
        public string SegmentNumber { get; set; }
    }
}
