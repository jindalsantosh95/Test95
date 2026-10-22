namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class BookingClassValidationRequest
    {
        public PreValidationPartnerData PreValidationBookingClass { get; set; }
        public string bookingClass { get; set; }
        public string flownClass { get; set; }
        public DateTime activityDate { get; set; }
        public string listFareValue { get; set; }
        public string listTicketValue { get; set; }
        public string listPaxValue { get; set; }

        public static explicit operator BookingClassValidationRequest((PreValidationPartnerData preValidationResponse, string bookingClass, string flownClass, DateTime activityDate, string listFareValue, string listPaxValue, string listTicketValue) input)
        {
            var (preValidationResponse, bookingClass, flownClass, activityDate, listFareValue, listPaxValue, listTicketValue) = input;

            return new BookingClassValidationRequest()
            {
                PreValidationBookingClass = preValidationResponse,
                bookingClass = bookingClass,
                flownClass = flownClass,
                activityDate = activityDate,
                listFareValue = listFareValue,
                listTicketValue = listTicketValue,
                listPaxValue = listPaxValue
            };
        }
    }
}
