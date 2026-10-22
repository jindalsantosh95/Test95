namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public sealed class HolidayPaymentResponseEnvelopeMs
    {
        public IReadOnlyList<HolidayPaymentResponseMs> PaymentSessions { get; set; }
    }
}
