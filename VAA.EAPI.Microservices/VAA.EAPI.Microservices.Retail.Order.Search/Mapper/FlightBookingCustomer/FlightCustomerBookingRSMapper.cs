using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.FlightCustomers;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper.FlightBookingCustomer
{
    public class FlightCustomerBookingRSMapper
    {
        public static FlightCustomer CustomerDetailMapper(List<Customer> customerDetails)
        {
            return new FlightCustomer()
            {
                Customer = customerDetails
            };
        }
    }
}