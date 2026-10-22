using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity
{
    public class Ticket
    {
        public string TicketStatus { get; set; }
        public string LastNameNumber { get; set; }
        public string FirstNameNumber { get; set; }
        public string PassengerName { get; set; }
        public string TicketNumber { get; set; }
        public string TicketType { get; set; }
        public string TicketIssueDate { get; set; }
        public string ElectronicTicketNumberFlag { get; set; }
        public string DigitCheck { get; set; }
        public string FreeformText { get; set; }
        public string TicketingTypeCode { get; set; }
        public string TicketType2 { get; set; }
        public string TicketType1 { get; set; }
        public string TicketName { get; set; }
        public string IssueDate { get; set; }
        public string ApprovalCode { get; set; }
        public string FormOfPayment { get; set; }
        public string FormOfPaymenttype { get; set; }
        public string CreditCardNumber { get; set; }
        public string CardHolderNameOrOther { get; set; }
        public string CreditCardExpiration { get; set; }
        public string AdditionalCollectionAmount { get; set; }
        public string ExchangedCurrencyAmount { get; set; }
        public string IataNumber { get; set; }
        public string OriginalFormOfPayment { get; set; }
        public string OriginalTicketCity { get; set; }
        public string OriginalTicketDate { get; set; }
        public string OriginalTicketNumber { get; set; }
        public string TotalExchangedAmount { get; set; }
        public string VoluntaryFlag { get; set; }
        public string OriginalSalesLocation { get; set; }
        public string OriginalTicketAgent { get; set; }
        public List<Coupon> Coupon { get; set; }
    }
}
