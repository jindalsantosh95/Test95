using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class CustomerMapper
    {
        public static List<CustomerDetails> Air4ToOrderChangeCustomerMapper(PerformRevenueResponseType performRevenueResponse, OrderChangeRequestData request, List<Passenger> passengers)
        {
            List<CustomerDetails> customers = new();
            foreach (var cd in request.CustomerDetails)
            {
                var response = passengers.Where(x => x.FirstName.ToUpper() == cd.FirstName.ToUpper() && x.LastName.ToUpper() == cd.LastName.ToUpper());
                foreach (var pd in response)
                {
                    customers.Add(new()
                    {
                        Title = pd.Title,
                        CustId = pd.CustomerId,
                        Ptc = cd.Ptc,
                        FirstName = pd.FirstName,
                        LastName = pd.LastName,
                        Gender = cd.Gender,
                        DateOfBirth = cd.DateOfBirth,
                        Contacts = new()
                        {
                            Email = cd.Contacts.Email,
                            Phone = cd.Contacts.Phone,
                            Countrycode = cd.Contacts.Countrycode
                        }
                    });
                }
            }
            return customers;
        }
    }
}
