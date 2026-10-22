using InfrastructureBookingCustomer = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.AppointmentBooking.BookingCustomer;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.AppointmentBooking;

public class BookingCustomer
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public static explicit operator InfrastructureBookingCustomer(BookingCustomer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new InfrastructureBookingCustomer
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone
        };
    }
}
