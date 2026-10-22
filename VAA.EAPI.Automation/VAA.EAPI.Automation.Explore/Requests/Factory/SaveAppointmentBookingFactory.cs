using VAA.EAPI.Automation.Explore.Requests.Models.RequestModels;

namespace VAA.EAPI.Automation.Explore.Requests.Factory
{
    public static class SaveAppointmentBookingFactory
    {
        public static Bookingdetails Create(
            string appointmentType,
            string appointmentReason,          
            DateOnly date,
            DateTimeOffset slotStart,
            DateTimeOffset slotEnd,
            string locationId,
            string firstName,
            string lastName,
            string email,
            string phone,
            string? bookingReference = null)
        {
            return new Bookingdetails
            {
                appointmentType = appointmentType,
                appointmentReason = appointmentReason,
                bookingReference = bookingReference,  // optional field

                date = date,
                slotStart = slotStart,
                slotEnd = slotEnd,

                locationId = locationId,

                visitPurpose = null,
                visitPurposeDetails = null,
                additionalDetails = null,

                customer = new Customer
                {
                    firstName = firstName,
                    lastName = lastName,
                    email = email,
                    phone = phone
                }
            };
        }
    }
}