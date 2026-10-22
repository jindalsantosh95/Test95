namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Payment
    {
        public string? TotalHolidayCost { get; set; }
        public string? OutstandingBalance { get; set; }
        public string? TotalPaid { get; set; }
        public string DueBy { get; set; }
        public List<PaymentHistory> PaymentHistory { get; set; }
        public string Currency { get; set; }
        public string? CancellationCharge { get; set; }
    }

}

