namespace VAA.EAPI.Domain.Virgin.Models.Ticket
{
    public class TicketInformation
    {
        public string BaseFare { get; set; }
        public string CurrencyCodeOfBaseFare { get; set; }
        public string TotalFare { get; set; }
        public string CurrencyCodeOfTotalFare { get; set; }
        public string FirstTicketNumber { get; set; }
        public string LastTicketNumber { get; set; }
    }
}
