using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper.FlightBookingCustomer
{
    public class CustomerDetailMapper
    {
        public static List<Customer> PassengerToCustomerMapper(List<Passenger> passengers, List<InfantInArms> infants, List<PassengerTypeInformation> ptc)
        {
            List<Customer> customers = new();
            if (infants.Any())
                passengers.AddRange(infants);
            foreach (var passenger in passengers)
            {
                customers.Add(new Customer()
                {
                    FirstName = passenger.FirstName,
                    LastName = passenger.LastName.Contains("/") ? passenger.LastName.Split("/").LastOrDefault() : passenger.LastName,
                    FirstNameNumber = passenger.FirstNameNumber,
                    LastNameNumber = passenger.LastNameNumber,
                    Title = passenger.Title,
                    CustId = passenger.CustomerId,
                    Ptc = ptc?.FirstOrDefault(x => x.FirstNameNumber == passenger.FirstNameNumber && x.LastNameNumber == passenger.LastNameNumber)?.PassengerTypeCode
                });
            }

            return customers;
        }
    }
}
